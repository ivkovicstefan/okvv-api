using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace OkVolleyVibes.Tests.Diagnostics;

/// <summary>End-to-end proof that endpoint → ISender → pipeline → handler is wired correctly.</summary>
public sealed class PingEndpointTests(TestingWebAppFactory factory) : IClassFixture<TestingWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Ping_with_a_message_round_trips_through_the_handler()
    {
        HttpResponseMessage response = await _client.GetAsync("/_diag/ping?message=hello");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("reply").GetString().Should().Be("pong: hello");
    }

    [Fact]
    public async Task Ping_without_a_message_is_rejected_by_the_validation_behavior()
    {
        HttpResponseMessage response = await _client.GetAsync("/_diag/ping");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement root = body.RootElement;
        root.GetProperty("errorCode").GetString().Should().Be("validation.failed");
        root.GetProperty("errors").GetProperty("Message")[0].GetString().Should().Be("A message is required.");
    }
}
