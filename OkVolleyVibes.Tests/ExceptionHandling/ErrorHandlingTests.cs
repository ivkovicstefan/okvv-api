using System.Net;
using System.Text.Json;

namespace OkVolleyVibes.Tests.ExceptionHandling;

public sealed class ErrorHandlingTests(TestingWebAppFactory factory)
    : IClassFixture<TestingWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Theory]
    [InlineData("notfound", 404, "diagnostic.not_found")]
    [InlineData("validation", 400, "validation.failed")]
    [InlineData("conflict", 409, "resource.conflict")]
    [InlineData("businessrule", 422, "business_rule.violation")]
    [InlineData("forbidden", 403, "access.forbidden")]
    public async Task Throwing_an_AppException_yields_problem_details(string kind, int status, string errorCode)
    {
        HttpResponseMessage response = await _client.GetAsync($"/_diag/throw/{kind}");

        ((int)response.StatusCode).Should().Be(status);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement root = body.RootElement;
        root.GetProperty("status").GetInt32().Should().Be(status);
        root.GetProperty("errorCode").GetString().Should().Be(errorCode);
        root.TryGetProperty("traceId", out _).Should().BeTrue();
        root.TryGetProperty("instance", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Validation_exception_exposes_field_errors()
    {
        HttpResponseMessage response = await _client.GetAsync("/_diag/throw/validation");

        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement name = body.RootElement.GetProperty("errors").GetProperty("name");
        name[0].GetString().Should().Be("Name is required.");
    }

    [Fact]
    public async Task Unexpected_exception_returns_500_without_leaking_details()
    {
        HttpResponseMessage response = await _client.GetAsync("/_diag/throw/unexpected");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement root = body.RootElement;
        root.GetProperty("errorCode").GetString().Should().Be("server.unexpected");
        root.GetProperty("detail").GetString().Should().Be("An unexpected error occurred.");
    }

    [Fact]
    public async Task Existing_health_endpoint_still_works()
    {
        HttpResponseMessage response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
