using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CookTime.Test;

/// <summary>
/// Tracks API calls made during tests and compares against OpenAPI spec to report coverage.
/// Usage:
///   1. Call ApiCoverageTracker.CreateClient(baseUrl) to get an HttpClient that tracks calls
///   2. Run your tests using that client
///   3. Call ApiCoverageTracker.GenerateReport(openApiUrl) to get coverage stats
/// </summary>
public static class ApiCoverageTracker
{
    private static readonly ConcurrentBag<(string Method, string Path)> _calledEndpoints = new();

    /// <summary>
    /// Creates an HttpClient that tracks all API calls for coverage reporting.
    /// </summary>
    public static HttpClient CreateClient(string baseUrl)
    {
        var handler = new TrackingHandler(new HttpClientHandler());
        return new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
    }

    /// <summary>
    /// Records an API call. Called automatically by TrackingHandler.
    /// </summary>
    public static void RecordCall(string method, string path)
    {
        // Normalize path: remove query string, trim trailing slash
        var normalizedPath = path.Split('?')[0].TrimEnd('/');
        if (string.IsNullOrEmpty(normalizedPath)) normalizedPath = "/";
        _calledEndpoints.Add((method.ToUpperInvariant(), normalizedPath));
    }

    /// <summary>
    /// Fetches the OpenAPI spec and generates a coverage report.
    /// </summary>
    public static async Task<CoverageReport> GenerateReportAsync(HttpClient client, string openApiPath = "/openapi/v1.json")
    {
        var response = await client.GetAsync(openApiPath);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        var spec = JsonDocument.Parse(json);
        var definedEndpoints = ParseOpenApiEndpoints(spec);

        var calledSet = _calledEndpoints
            .Select(e => (e.Method, NormalizePath(e.Path)))
            .Distinct()
            .ToHashSet();

        var covered = new List<(string Method, string Path)>();
        var notCovered = new List<(string Method, string Path)>();

        foreach (var endpoint in definedEndpoints)
        {
            // Check if any called endpoint matches this defined endpoint (considering path parameters)
            var isCovered = calledSet.Any(called =>
                called.Method == endpoint.Method &&
                PathMatches(called.Item2, endpoint.Path));

            if (isCovered)
            {

                covered.Add(endpoint);
            }
            else
            {
                notCovered.Add(endpoint);
            }
        }

        return new CoverageReport
        {
            TotalEndpoints = definedEndpoints.Count,
            CoveredEndpoints = covered.Count,
            CoveragePercent = definedEndpoints.Count > 0
                ? (double)covered.Count / definedEndpoints.Count * 100
                : 100,
            Covered = covered,
            NotCovered = notCovered
        };
    }

    /// <summary>
    /// Clears all tracked calls. Call this between test runs if needed.
    /// </summary>
    public static void Reset()
    {
        _calledEndpoints.Clear();
    }

    private static List<(string Method, string Path)> ParseOpenApiEndpoints(JsonDocument spec)
    {
        var endpoints = new List<(string Method, string Path)>();

        if (!spec.RootElement.TryGetProperty("paths", out var paths))
            return endpoints;

        foreach (var pathProperty in paths.EnumerateObject())
        {
            var path = pathProperty.Name;
            foreach (var methodProperty in pathProperty.Value.EnumerateObject())
            {
                var method = methodProperty.Name.ToUpperInvariant();
                // Skip non-HTTP methods like "parameters", "summary", etc.
                if (method is "GET" or "POST" or "PUT" or "DELETE" or "PATCH" or "HEAD" or "OPTIONS")
                {
                    endpoints.Add((method, path));
                }
            }
        }

        return endpoints;
    }

    private static string NormalizePath(string path)
    {
        return path.Split('?')[0].TrimEnd('/');
    }

    /// <summary>
    /// Checks if a called path matches an OpenAPI path pattern.
    /// Handles path parameters like /api/recipe/{id} matching /api/recipe/123
    /// </summary>
    private static bool PathMatches(string calledPath, string patternPath)
    {
        // Convert OpenAPI path pattern to regex
        // /api/recipe/{id} -> ^/api/recipe/[^/]+$
        var regexPattern = "^" + Regex.Replace(patternPath, @"\{[^}]+\}", "[^/]+") + "$";
        return Regex.IsMatch(calledPath, regexPattern, RegexOptions.IgnoreCase);
    }

    private class TrackingHandler : DelegatingHandler
    {
        public TrackingHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var method = request.Method.Method;
            var path = request.RequestUri?.PathAndQuery ?? "/";
            RecordCall(method, path);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}

public class CoverageReport
{
    public int TotalEndpoints { get; init; }
    public int CoveredEndpoints { get; init; }
    public double CoveragePercent { get; init; }
    public List<(string Method, string Path)> Covered { get; init; } = new();
    public List<(string Method, string Path)> NotCovered { get; init; } = new();

    public void PrintToConsole()
    {
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("                    API COVERAGE REPORT                     ");
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine($"  Total Endpoints:   {TotalEndpoints}");
        Console.WriteLine($"  Covered:           {CoveredEndpoints}");
        Console.WriteLine($"  Coverage:          {CoveragePercent:F1}%");
        Console.WriteLine("───────────────────────────────────────────────────────────");

        if (NotCovered.Count > 0)
        {
            Console.WriteLine("  NOT COVERED:");
            foreach (var (method, path) in NotCovered.OrderBy(e => e.Path).ThenBy(e => e.Method))
            {
                Console.WriteLine($"    {method,-7} {path}");
            }
        }
        else
        {
            Console.WriteLine("  ✓ All endpoints covered!");
        }

        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine();
    }
}
