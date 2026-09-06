using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OkVolleyVibes.Domain.Identity;
using OkVolleyVibes.Infrastructure.Identity;
using OkVolleyVibes.Infrastructure.Persistence;

namespace OkVolleyVibes.Tests.Authentication;

public sealed class RegistrationTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static HttpContent Body(
        string email,
        string password = "a-good-long-passphrase-42",
        string firstName = "Test",
        string lastName = "User",
        string phone = "+381641234567")
        => JsonContent.Create(new { firstName, lastName, email, phoneNumber = phone, password });

    [Fact]
    public async Task Register_creates_a_player_with_a_profile_and_sends_a_verification_email()
    {
        const string email = "reg.player@example.com";

        HttpResponseMessage response = await _client.PostAsync("/api/auth/register", Body(email));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        JsonElement payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Guid userId = payload.GetProperty("userId").GetGuid();

        factory.Email.CountTo(email).Should().Be(1);

        using IServiceScope scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        User user = (await users.FindByIdAsync(userId.ToString()))!;
        user.Should().NotBeNull();
        (await users.IsInRoleAsync(user, Roles.Player)).Should().BeTrue();
        (await users.IsEmailConfirmedAsync(user)).Should().BeFalse();
        (await db.PlayerProfiles.AnyAsync(p => p.UserId == userId)).Should().BeTrue();
    }

    [Fact]
    public async Task Register_with_an_email_that_is_already_taken_returns_409()
    {
        const string email = "reg.dup@example.com";
        await _client.PostAsync("/api/auth/register", Body(email));

        HttpResponseMessage response = await _client.PostAsync("/api/auth/register", Body(email));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        JsonElement payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("errorCode").GetString().Should().Be("account.email_in_use");
    }

    [Fact]
    public async Task Register_with_a_common_password_returns_400()
    {
        HttpResponseMessage response = await _client.PostAsync(
            "/api/auth/register", Body("reg.weak@example.com", password: "password123"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        JsonElement payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("errorCode").GetString().Should().Be("validation.failed");
        payload.GetProperty("errors").GetProperty("password")[0].GetString().Should().Contain("too common");
    }

    [Fact]
    public async Task Register_validation_messages_follow_the_Accept_Language_header()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/register")
        {
            Content = JsonContent.Create(new
            {
                firstName = "",
                lastName = "User",
                email = "not-an-email",
                phoneNumber = "",
                password = "x",
            }),
        };
        request.Headers.Add("Accept-Language", "sr-Latn");

        HttpResponseMessage response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        JsonElement payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("errors").GetProperty("FirstName")[0].GetString().Should().Be("Ime je obavezno.");
        payload.GetProperty("errors").GetProperty("Password")[0].GetString().Should().Be("Lozinka mora imati najmanje 10 znakova.");
    }
}
