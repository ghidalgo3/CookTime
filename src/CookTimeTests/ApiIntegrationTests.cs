namespace Tests;

/// <summary>
/// Integration tests that hit the running API server via HTTP.
/// Requires the server to be running (scripts/server).
/// </summary>
[TestClass]
public class ApiIntegrationTests
{
    private static readonly string BaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5001";
    private static HttpClient _client = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        _client = ApiCoverageTracker.CreateClient(BaseUrl);
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        // Generate and print coverage report at the end of all tests
        var report = await ApiCoverageTracker.GenerateReportAsync(_client);
        report.PrintToConsole();
        _client.Dispose();
    }

    [TestMethod]
    public async Task Health_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetCategories_ReturnsDefaultCategories()
    {
        var response = await _client.GetAsync("/api/category/list");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.IsFalse(string.IsNullOrEmpty(content));
    }

    [TestMethod]
    public async Task GetRecipeTags_ReturnsCategories()
    {
        var response = await _client.GetAsync("/api/recipe/tags");
        response.EnsureSuccessStatusCode();
    }

    [TestMethod]
    public async Task SearchRecipeTags_WithQuery_ReturnsFiltered()
    {
        var response = await _client.GetAsync("/api/recipe/tags?query=dinner");
        response.EnsureSuccessStatusCode();
    }

    [TestMethod]
    public async Task GetUnits_ReturnsUnitList()
    {
        var response = await _client.GetAsync("/api/recipe/units");
        response.EnsureSuccessStatusCode();
    }

    [TestMethod]
    public async Task SearchIngredients_ReturnsResults()
    {
        var response = await _client.GetAsync("/api/ingredient?query=tomato");
        response.EnsureSuccessStatusCode();
    }

    [TestMethod]
    public async Task GetRecipes_ReturnsPaginatedList()
    {
        var response = await _client.GetAsync("/api/multipartrecipe");
        response.EnsureSuccessStatusCode();
    }

    [TestMethod]
    public async Task GetRecipes_WithSearch_ReturnsFiltered()
    {
        var response = await _client.GetAsync("/api/multipartrecipe?search=pasta");
        response.EnsureSuccessStatusCode();
    }

    [TestMethod]
    public async Task GetNewRecipes_ReturnsRecentRecipes()
    {
        var response = await _client.GetAsync("/api/multipartrecipe/new");
        response.EnsureSuccessStatusCode();
    }

    [TestMethod]
    public async Task GetFeaturedRecipes_ReturnsFeatured()
    {
        var response = await _client.GetAsync("/api/multipartrecipe/featured");
        response.EnsureSuccessStatusCode();
    }

    [TestMethod]
    public async Task GetRecipeById_WithInvalidId_ReturnsNotFound()
    {
        var fakeId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/multipartrecipe/{fakeId}");
        Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task GetProfile_Unauthenticated_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/account/profile");
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetMyRecipes_Unauthenticated_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/multipartrecipe/mine");
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetMyLists_Unauthenticated_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/lists");
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetSitemap_ReturnsXml()
    {
        var response = await _client.GetAsync("/sitemap.xml");
        response.EnsureSuccessStatusCode();

        var contentType = response.Content.Headers.ContentType?.MediaType;
        Assert.IsTrue(contentType?.Contains("xml") ?? false, $"Expected XML content type, got {contentType}");
    }

    [TestMethod]
    public async Task OpenApi_ReturnsSpec()
    {
        var response = await _client.GetAsync("/openapi/v1.json");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("\"paths\""), "OpenAPI spec should contain paths");
    }
}
