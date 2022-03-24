using Microsoft.EntityFrameworkCore;
using babe_algorithms.Services;
using SixLabors.ImageSharp.Web.DependencyInjection;

namespace babe_algorithms;
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews().AddNewtonsoftJson();
        services.AddRazorPages();
        services.AddScoped<IUserService, UserService>();
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = this.Configuration.GetConnectionString("Postgres");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString =
                    Environment.GetEnvironmentVariable("POSTGRESQLCONNSTR_Postgres")
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
        app.UseStaticFiles();
        app.UseRouting();
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
