using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Npgsql;

namespace babe_algorithms.Services;

public static class GoogleAuth
{
    public static IServiceCollection AddGoogleAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure forwarded headers for Docker/proxy scenarios
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.LoginPath = "/signin";
            options.LogoutPath = "/api/auth/signout";
        })
        .AddGoogle(options =>
        {
            options.ClientId = configuration["Google:ClientId"] ?? "";
            options.ClientSecret = configuration["Google:ClientSecret"] ?? "";
            options.CallbackPath = "/api/auth/google-callback";

            // Override redirect URI for Docker/proxy scenarios
            var baseUrl = configuration["Google:BaseUrl"];
            if (!string.IsNullOrEmpty(baseUrl))
            {
                var callbackUrl = $"{baseUrl}/api/auth/google-callback";

                // Fix redirect_uri in authorization request
                options.Events.OnRedirectToAuthorizationEndpoint = context =>
                {
                    var uri = new UriBuilder(context.RedirectUri);
                    var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    query["redirect_uri"] = callbackUrl;
                    uri.Query = query.ToString();
                    context.Response.Redirect(uri.ToString());
                    return Task.CompletedTask;
                };

                // Fix redirect_uri in token exchange request
                options.Events.OnCreatingTicket = context =>
                {
                    return Task.CompletedTask;
                };

                // Use custom backchannel handler to fix token request
                var innerHandler = new HttpClientHandler();
                options.BackchannelHttpHandler = new RedirectUriFixingHandler(innerHandler, callbackUrl);
            }
        });

        return services;
    }

    public static WebApplication MapGoogleAuthEndpoints(this WebApplication app)
    {
        app.UseAuthentication();

        app.MapGet("/api/auth/google", () =>
            Results.Challenge(
                new AuthenticationProperties { RedirectUri = "/api/auth/google-success" },
                [GoogleDefaults.AuthenticationScheme]));

        app.MapGet("/api/auth/google-success", async (HttpContext context, NpgsqlDataSource dataSource) =>
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                return Results.Redirect("/signin");
            }

            var googleId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var email = context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var name = context.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            if (googleId == null)
            {
                return Results.Redirect("/signin");
            }

            // Upsert user in database
            await using var conn = await dataSource.OpenConnectionAsync();
            await using var cmd = new NpgsqlCommand(@"
                INSERT INTO cooktime.users (provider, provider_user_id, email, display_name, last_login_date)
                VALUES ('google', $1, $2, $3, now())
                ON CONFLICT (provider, provider_user_id) 
                DO UPDATE SET email = $2, display_name = $3, last_login_date = now()
                RETURNING id, roles", conn);

            cmd.Parameters.AddWithValue(googleId);
            cmd.Parameters.AddWithValue(email ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue(name ?? "Google User");

            Guid dbUserId;
            string[] roles = ["User"];
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    dbUserId = reader.GetGuid(0);
                    roles = reader.GetFieldValue<string[]>(1);
                }
                else
                {
                    return Results.Redirect("/signin");
                }
            }

            // Sign in with a new identity that includes the database user ID and roles
            var claims = new List<System.Security.Claims.Claim>
            {
                new("db_user_id", dbUserId.ToString()),
                new(System.Security.Claims.ClaimTypes.Name, name ?? "Google User"),
                new(System.Security.Claims.ClaimTypes.Email, email ?? ""),
            };
            foreach (var role in roles)
            {
                claims.Add(new(System.Security.Claims.ClaimTypes.Role, role));
            }
            var identity = new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);

            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return Results.Redirect("/");
        });

        app.MapGet("/api/auth/signout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/");
        });

        app.MapGet("/api/account/profile", (HttpContext context) =>
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }

            var name = context.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var email = context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var id = context.User.FindFirst("db_user_id")?.Value;
            var roles = context.User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToArray();

            return Results.Ok(new
            {
                name = name ?? email ?? "User",
                id = id,
                csrfToken = "",
                roles = roles.Length > 0 ? roles : new[] { "User" }
            });
        });

        return app;
    }
}

/// <summary>
/// HTTP handler that rewrites the redirect_uri in OAuth token exchange requests.
/// This is needed when running behind a proxy/Docker where the internal URL differs from the external URL.
/// </summary>
public class RedirectUriFixingHandler : DelegatingHandler
{
    private readonly string _correctRedirectUri;

    public RedirectUriFixingHandler(HttpMessageHandler innerHandler, string correctRedirectUri)
        : base(innerHandler)
    {
        _correctRedirectUri = correctRedirectUri;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content is FormUrlEncodedContent)
        {
            var content = await request.Content.ReadAsStringAsync(cancellationToken);
            var parameters = System.Web.HttpUtility.ParseQueryString(content);

            if (parameters["redirect_uri"] != null)
            {
                parameters["redirect_uri"] = _correctRedirectUri;
                request.Content = new FormUrlEncodedContent(
                    parameters.AllKeys.Select(k => new KeyValuePair<string, string>(k!, parameters[k]!)));
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
