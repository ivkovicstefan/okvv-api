using System.Net;
using System.Net.Http.Json;

namespace OkVolleyVibes.Tests.Authentication;

public sealed class ResendVerificationTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
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
    public async Task Resending_for_an_unverified_account_sends_another_email()
    {
        const string email = "resend.unverified@example.com";
        await _client.PostAsync("/api/auth/register", Body(email));
        int before = factory.Email.CountTo(email);

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/auth/resend-verification", new { email });

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        factory.Email.CountTo(email).Should().Be(before + 1);
    }

    [Fact]
    public async Task Resending_for_an_unknown_email_is_accepted_and_sends_nothing()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/auth/resend-verification", new { email = "resend.ghost@example.com" });

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        factory.Email.CountTo("resend.ghost@example.com").Should().Be(0);
    }

    [Fact]
    public async Task Resending_after_verification_is_accepted_and_sends_nothing()
    {
        const string email = "resend.verified@example.com";
        await _client.PostAsync("/api/auth/register", Body(email));
        await _client.GetAsync(factory.Email.VerificationPathFor(email));
        int before = factory.Email.CountTo(email);

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/auth/resend-verification", new { email });

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        factory.Email.CountTo(email).Should().Be(before);
    }
}
