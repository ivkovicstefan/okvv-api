using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace OkVolleyVibes.Tests.Authentication;

public sealed class TokenFlowTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Verifying_the_email_signs_the_user_in()
    {
        AuthApiFactory.AuthedUser user = await factory.RegisterAndVerifyAsync("tf.verify@example.com");

        user.AccessToken.Should().NotBeNullOrWhiteSpace();
        user.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Refresh_rotates_the_token_and_the_old_one_stops_working()
    {
        AuthApiFactory.AuthedUser user = await factory.RegisterAndVerifyAsync("tf.rotate@example.com");

        HttpResponseMessage first = await _client.PostAsJsonAsync(
            "/api/auth/refresh", new { refreshToken = user.RefreshToken });
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        string rotated = (await first.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("refreshToken").GetString()!;
        rotated.Should().NotBe(user.RefreshToken);

        HttpResponseMessage reuse = await _client.PostAsJsonAsync(
            "/api/auth/refresh", new { refreshToken = user.RefreshToken });
        reuse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        HttpResponseMessage withRotated = await _client.PostAsJsonAsync(
            "/api/auth/refresh", new { refreshToken = rotated });
        withRotated.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Logout_revokes_the_refresh_token()
    {
        AuthApiFactory.AuthedUser user = await factory.RegisterAndVerifyAsync("tf.logout@example.com");

        HttpResponseMessage logout = await _client.PostAsJsonAsync(
            "/api/auth/logout", new { refreshToken = user.RefreshToken });
        logout.StatusCode.Should().Be(HttpStatusCode.NoContent);

        HttpResponseMessage afterLogout = await _client.PostAsJsonAsync(
            "/api/auth/refresh", new { refreshToken = user.RefreshToken });
        afterLogout.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task An_invalid_bearer_token_is_rejected()
    {
        HttpResponseMessage response = await factory.CreateClient("not-a-real-jwt").GetAsync("/api/account/profile");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
