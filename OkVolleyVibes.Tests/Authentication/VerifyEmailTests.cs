using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OkVolleyVibes.Infrastructure.Identity;

namespace OkVolleyVibes.Tests.Authentication;

public sealed class VerifyEmailTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private HttpContent Body(string email)
        => JsonContent.Create(new
        {
            firstName = "Test",
            lastName = "User",
            email,
            phoneNumber = "+381641234567",
            password = "a-good-long-passphrase-42",
        });

    [Fact]
    public async Task The_link_from_the_email_confirms_the_address()
    {
        const string email = "verify.ok@example.com";
        await _client.PostAsync("/api/auth/register", Body(email));

        HttpResponseMessage response = await _client.GetAsync(factory.Email.VerificationPathFor(email));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        JsonElement tokens = await response.Content.ReadFromJsonAsync<JsonElement>();
        tokens.GetProperty("accessToken").GetString().Should().NotBeNullOrWhiteSpace();
        tokens.GetProperty("refreshToken").GetString().Should().NotBeNullOrWhiteSpace();

        using IServiceScope scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        User user = (await users.FindByEmailAsync(email))!;
        (await users.IsEmailConfirmedAsync(user)).Should().BeTrue();
    }

    [Fact]
    public async Task A_garbage_token_for_a_real_user_returns_400()
    {
        JsonElement payload = await (await _client.PostAsync("/api/auth/register", Body("verify.badtoken@example.com")))
            .Content.ReadFromJsonAsync<JsonElement>();
        Guid userId = payload.GetProperty("userId").GetGuid();

        HttpResponseMessage response = await _client.GetAsync(
            $"/api/auth/verify-email?userId={userId}&token=not-a-valid-token");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        JsonElement error = await response.Content.ReadFromJsonAsync<JsonElement>();
        error.GetProperty("errorCode").GetString().Should().Be("validation.failed");
    }

    [Fact]
    public async Task An_unknown_user_returns_404()
    {
        HttpResponseMessage response = await _client.GetAsync(
            $"/api/auth/verify-email?userId={Guid.NewGuid()}&token=whatever");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
