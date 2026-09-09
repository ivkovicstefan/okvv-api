using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using OkVolleyVibes.Tests.Authentication;

namespace OkVolleyVibes.Tests.Onboarding;

public sealed class OnboardingGateTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    [Fact]
    public async Task No_token_is_401()
    {
        HttpResponseMessage response = await factory.CreateClient().GetAsync("/_diag/whoami");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Signed_in_but_onboarding_unfinished_is_403_profile_incomplete()
    {
        AuthApiFactory.AuthedUser user = await factory.RegisterAndVerifyAsync("gate.incomplete@example.com");

        HttpResponseMessage response = await factory.CreateClient(user.AccessToken).GetAsync("/_diag/whoami");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("errorCode").GetString().Should().Be("profile.incomplete");
    }

    [Fact]
    public async Task Passes_once_the_profile_is_complete()
    {
        AuthApiFactory.AuthedUser user = await factory.RegisterAndVerifyAsync("gate.complete@example.com");
        HttpClient client = factory.CreateClient(user.AccessToken);

        JsonElement tokens = await (await client.PostAsJsonAsync("/api/account/complete-profile", new
        {
            dateOfBirth = "1994-03-03",
            preferredLanguage = "en",
            skillRating = 6,
            hasTrainedBefore = true,
            trainingHistory = "Local club, 2010-2014",
            positions = new[] { "MiddleBlocker" },
            recreationalExperience = (string?)null,
            clubInterest = "Both",
            agreedToClubRules = true,
        })).Content.ReadFromJsonAsync<JsonElement>();

        HttpResponseMessage response = await factory
            .CreateClient(tokens.GetProperty("accessToken").GetString()!)
            .GetAsync("/_diag/whoami");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
