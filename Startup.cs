using Microsoft.EntityFrameworkCore;
using babe_algorithms.Services;
using SixLabors.ImageSharp.Web.DependencyInjection;
using GustavoTech.Implementation;
using Microsoft.AspNetCore.Identity.UI.Services;
using babe_algorithms.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace babe_algorithms;
public class Startup
{
    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        this.Configuration = configuration;
        this.Environment = env;
    }

    public IConfiguration Configuration { get; }

    public IWebHostEnvironment Environment { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews().AddNewtonsoftJson();
        var mvcBuilder = services.AddRazorPages();
        if (this.Environment.IsDevelopment())
        {
            mvcBuilder.AddRazorRuntimeCompilation();
        }
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 10;
            options.Password.RequiredUniqueChars = 2;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.SignIn.RequireConfirmedEmail = true;
        })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        services.ConfigureApplicationCookie(options => 
        {
            // Cookie settings
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(1);

            options.LoginPath = "/SignIn";
            options.AccessDeniedPath = "/SignIn";
            options.SlidingExpiration = true;
        });

        services.AddScoped<ISignInManager, SignInManager>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEmailSender, EmailSender>();
        services.Configure<AuthMessageSenderOptions>(this.Configuration);
        services.AddTransient<IEmailSender, EmailSender>();
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = this.Configuration.GetConnectionString("Postgres");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString =
                    System.Environment.GetEnvironmentVariable("POSTGRESQLCONNSTR_Postgres")
                    ?? throw new NullReferenceException("Connection string was not found.");
            }
            options.UseNpgsql(connectionString);
            options.EnableSensitiveDataLogging(true);
        });
        services.AddImageSharp(options => 
        {
        })
            .ClearProviders()
            .AddProvider<PostgresImageProvider>(sp => 
            {
                return new PostgresImageProvider(sp);
            });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.Map(
                "/js", ctx =>
                {
                    ctx.UseSpa(spa =>
                    {
                        spa.UseProxyToSpaDevelopmentServer("http://localhost:8080/js");
                    });
                });
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        app.UseImageSharp();
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            endpoints.MapRazorPages();
        });
    }
}
