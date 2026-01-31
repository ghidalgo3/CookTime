using System.Text;
using BabeAlgorithms.Services;
using Microsoft.Extensions.Caching.Memory;

namespace BabeAlgorithms.Routes;

public static class SitemapRoutes
{
    private const string SitemapCacheKey = "sitemap_xml";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);
    private const string BaseUrl = "https://letscooktime.com";

    public static IEndpointRouteBuilder MapSitemapRoutes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/sitemap.xml", async (CookTimeDB cooktime, IMemoryCache cache) =>
        {
            if (cache.TryGetValue(SitemapCacheKey, out string? cachedSitemap) && cachedSitemap != null)
            {
                return Results.Content(cachedSitemap, "application/xml");
            }

            var sitemap = await GenerateSitemapAsync(cooktime);
            cache.Set(SitemapCacheKey, sitemap, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration
            });

            return Results.Content(sitemap, "application/xml");
        });

        return app;
    }

    private static async Task<string> GenerateSitemapAsync(CookTimeDB cooktime)
    {
        var recipes = await cooktime.GetRecipesForSitemapAsync();

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        // Static pages
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{BaseUrl}/</loc>");
        sb.AppendLine("  </url>");

        // Recipe pages
        foreach (var recipe in recipes)
        {
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{BaseUrl}/recipes/details?id={recipe.Id}</loc>");
            sb.AppendLine($"    <lastmod>{recipe.LastModified:yyyy-MM-dd}</lastmod>");
            sb.AppendLine("  </url>");
        }

        sb.AppendLine("</urlset>");

        return sb.ToString();
    }
}
