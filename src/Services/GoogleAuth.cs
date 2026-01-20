using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Npgsql;

namespace babe_algorithms.Services;

public record UpdateProfileRequest(string DisplayName);

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
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        })
        .AddGoogle(options =>
        {
            options.ClientId = configuration["Google:ClientId"] ?? "";
            options.ClientSecret = configuration["Google:ClientSecret"] ?? "";
            options.CallbackPath = "/api/auth/google-callback";

            // Fix correlation cookie for OAuth redirect
            // Use Lax for local HTTP development, None+Secure for production HTTPS
            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            if (isDevelopment)
            {
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            }
            else
            {
                options.CorrelationCookie.SameSite = SameSiteMode.None;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
            }

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
            // First, try to claim a migrated cooktime user by email (converts them to google auth)
            // Then fall back to normal google user upsert
            await using var conn = await dataSource.OpenConnectionAsync();
            await using var claimMigratedCmd = new NpgsqlCommand(@"
                UPDATE cooktime.users 
                SET provider = 'google', provider_user_id = $1, last_login_date = now()
                WHERE email = $2 AND provider = 'cooktime'
                RETURNING id, roles, display_name", conn);

            claimMigratedCmd.Parameters.AddWithValue(googleId);
            claimMigratedCmd.Parameters.AddWithValue(email ?? (object)DBNull.Value);

            Guid dbUserId = Guid.Empty;
            string[] roles = ["User"];
            string? displayName = null;
            bool foundUser = false;

            await using (var reader = await claimMigratedCmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    dbUserId = reader.GetGuid(0);
                    roles = reader.GetFieldValue<string[]>(1);
                    displayName = reader.IsDBNull(2) ? null : reader.GetString(2);
                    foundUser = true;
                }
            }

            // If no migrated user was claimed, do normal google upsert
            if (!foundUser)
            {
                // For new users, don't set display_name - they'll be prompted to choose one
                // For returning users, preserve their existing display_name
                await using var upsertCmd = new NpgsqlCommand(@"
                    INSERT INTO cooktime.users (provider, provider_user_id, email, last_login_date)
                    VALUES ('google', $1, $2, now())
                    ON CONFLICT (provider, provider_user_id) 
                    DO UPDATE SET email = $2, last_login_date = now()
                    RETURNING id, roles, display_name", conn);

                upsertCmd.Parameters.AddWithValue(googleId);
                upsertCmd.Parameters.AddWithValue(email ?? (object)DBNull.Value);

                await using var reader = await upsertCmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    dbUserId = reader.GetGuid(0);
                    roles = reader.GetFieldValue<string[]>(1);
                    displayName = reader.IsDBNull(2) ? null : reader.GetString(2);
                    foundUser = true;
                }
            }

            if (!foundUser)
            {
                return Results.Redirect("/signin");
            }

            // Sign in with a new identity that includes the database user ID and roles
            var claims = new List<System.Security.Claims.Claim>
            {
                new("db_user_id", dbUserId.ToString()),
                new(System.Security.Claims.ClaimTypes.Name, displayName ?? name ?? "Google User"),
                new(System.Security.Claims.ClaimTypes.Email, email ?? ""),
            };
            foreach (var role in roles)
            {
                claims.Add(new(System.Security.Claims.ClaimTypes.Role, role));
            }
            var identity = new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);

            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Redirect to profile setup if they haven't set a display name yet
            if (string.IsNullOrEmpty(displayName))
            {
                return Results.Redirect("/profile/setup");
            }

            return Results.Redirect("/");
        });

        app.MapGet("/api/auth/signout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/");
        });

        app.MapGet("/api/account/profile", async (HttpContext context, NpgsqlDataSource dataSource) =>
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }

            var email = context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var id = context.User.FindFirst("db_user_id")?.Value;

            // Fetch display_name and roles from database
            string[] roles = ["User"];
            string? displayName = null;
            if (Guid.TryParse(id, out var userId))
            {
                await using var conn = await dataSource.OpenConnectionAsync();
                await using var cmd = new NpgsqlCommand("SELECT display_name, roles FROM cooktime.users WHERE id = $1", conn);
                cmd.Parameters.AddWithValue(userId);
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    displayName = reader.IsDBNull(0) ? null : reader.GetString(0);
                    roles = reader.GetFieldValue<string[]>(1);
                }
            }

            return Results.Ok(new
            {
                name = displayName ?? email ?? "User",
                id = id,
                csrfToken = "",
                roles = roles.Length > 0 ? roles : new[] { "User" },
                needsProfileSetup = string.IsNullOrEmpty(displayName)
            });
        });

        app.MapPut("/api/account/profile", async (HttpContext context, NpgsqlDataSource dataSource) =>
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }

            var id = context.User.FindFirst("db_user_id")?.Value;
            if (!Guid.TryParse(id, out var userId))
            {
                return Results.BadRequest("Invalid user ID");
            }

            var body = await context.Request.ReadFromJsonAsync<UpdateProfileRequest>();
            if (body == null || string.IsNullOrWhiteSpace(body.DisplayName))
            {
                return Results.BadRequest("Display name is required");
            }

            var displayName = body.DisplayName.Trim();
            if (displayName.Length < 2 || displayName.Length > 50)
            {
                return Results.BadRequest("Display name must be between 2 and 50 characters");
            }

            await using var conn = await dataSource.OpenConnectionAsync();

            // Check if display name is already taken
            await using var checkCmd = new NpgsqlCommand(
                "SELECT id FROM cooktime.users WHERE display_name = $1 AND id != $2", conn);
            checkCmd.Parameters.AddWithValue(displayName);
            checkCmd.Parameters.AddWithValue(userId);
            var existing = await checkCmd.ExecuteScalarAsync();
            if (existing != null)
            {
                return Results.Conflict(new { message = "This username is already taken" });
            }

            // Update the display name
            await using var updateCmd = new NpgsqlCommand(
                "UPDATE cooktime.users SET display_name = $1 WHERE id = $2", conn);
            updateCmd.Parameters.AddWithValue(displayName);
            updateCmd.Parameters.AddWithValue(userId);
            await updateCmd.ExecuteNonQueryAsync();

            return Results.Ok(new { displayName });
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
