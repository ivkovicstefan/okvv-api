using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using OkVolleyVibes.Tests.Authentication;

namespace OkVolleyVibes.Tests.Onboarding;

public sealed class CompleteProfileTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    private static object TrainedSurvey() => new
    {
        dateOfBirth = "1996-04-12",
        preferredLanguage = "sr-Latn",
        skillRating = 8,
        hasTrainedBefore = true,
        trainingHistory = "OK Partizan 2012-2017",
        positions = new[] { "Setter", "Opposite" },
        recreationalExperience = (string?)null,
        clubInterest = "Competitive",
        agreedToClubRules = true,
    };

    private static object CasualSurvey() => new
    {
        dateOfBirth = "2001-09-01",
        preferredLanguage = "en",
        skillRating = 3,
        hasTrainedBefore = false,
        trainingHistory = (string?)null,
        positions = Array.Empty<string>(),
        recreationalExperience = "AFewTimes",
        clubInterest = "Recreational",
        agreedToClubRules = true,
    };

    [Fact]
    public async Task Completing_the_survey_returns_tokens_that_pass_the_gate()
    {
        AuthApiFactory.AuthedUser user = await factory.RegisterAndVerifyAsync("cp.trained@example.com");
        HttpClient client = factory.CreateClient(user.AccessToken);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/account/complete-profile", TrainedSurvey());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        JsonElement tokens = await response.Content.ReadFromJsonAsync<JsonElement>();
        string newAccess = tokens.GetProperty("accessToken").GetString()!;

        HttpResponseMessage gated = await factory.CreateClient(newAccess).GetAsync("/_diag/whoami");
        gated.StatusCode.Should().Be(HttpStatusCode.OK);

        JsonElement profile = await factory.CreateClient(newAccess)
            .GetFromJsonAsync<JsonElement>("/api/account/profile");
        profile.GetProperty("profileCompleted").GetBoolean().Should().BeTrue();
        profile.GetProperty("preferredLanguage").GetString().Should().Be("sr-Latn");
        profile.GetProperty("survey").GetProperty("positions").EnumerateArray()
            .Select(p => p.GetString()).Should().BeEquivalentTo("Setter", "Opposite");
    }

    [Fact]
    public async Task A_casual_player_answers_the_recreational_branch()
    {
        AuthApiFactory.AuthedUser user = await factory.RegisterAndVerifyAsync("cp.casual@example.com");

        HttpResponseMessage response = await factory.CreateClient(user.AccessToken)
            .PostAsJsonAsync("/api/account/complete-profile", CasualSurvey());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Submitting_twice_returns_409()
    {
        AuthApiFactory.AuthedUser user = await factory.RegisterAndVerifyAsync("cp.twice@example.com");
        HttpClient client = factory.CreateClient(user.AccessToken);

        (await client.PostAsJsonAsync("/api/account/complete-profile", TrainedSurvey()))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        HttpResponseMessage second = await client.PostAsJsonAsync("/api/account/complete-profile", CasualSurvey());
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Validation_enforces_rules_agreement_skill_range_and_branch_fields_and_is_localized()
    {
        AuthApiFactory.AuthedUser user = await factory.RegisterAndVerifyAsync("cp.invalid@example.com");
        HttpClient client = factory.CreateClient(user.AccessToken);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/account/complete-profile")
        {
            Content = JsonContent.Create(new
            {
                dateOfBirth = "1990-01-01",
                preferredLanguage = "en",
                skillRating = 42,
                hasTrainedBefore = false,
                trainingHistory = (string?)null,
                positions = Array.Empty<string>(),
                recreationalExperience = (string?)null,
                clubInterest = "Recreational",
                agreedToClubRules = false,
            }),
        };
        request.Headers.Add("Accept-Language", "sr-Latn");

        HttpResponseMessage response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        JsonElement errors = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors");
        errors.GetProperty("SkillRating")[0].GetString().Should().Be("Ocenite svoje umeće ocenom od 1 do 10.");
        errors.GetProperty("RecreationalExperience")[0].GetString().Should().Be("Recite nam koliko ste rekreativno igrali odbojku.");
        errors.GetProperty("AgreedToClubRules")[0].GetString().Should().Be("Morate prihvatiti pravila kluba da biste nastavili.");
    }

    [Fact]
    public async Task The_endpoint_needs_a_signed_in_user()
    {
        HttpResponseMessage response = await factory.CreateClient()
            .PostAsJsonAsync("/api/account/complete-profile", TrainedSurvey());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task A_malformed_body_is_400_not_500()
    {
        AuthApiFactory.AuthedUser user = await factory.RegisterAndVerifyAsync("cp.malformed@example.com");

        // Unknown enum value + wrong type for recreationalExperience.
        var badJson = new StringContent(
            """{"dateOfBirth":"2001-01-01","preferredLanguage":"en","skillRating":5,"hasTrainedBefore":true,"trainingHistory":"x","positions":["OH"],"recreationalExperience":true,"clubInterest":"Recreational","agreedToClubRules":true}""",
            System.Text.Encoding.UTF8,
            "application/json");

        HttpResponseMessage response = await factory.CreateClient(user.AccessToken)
            .PostAsync("/api/account/complete-profile", badJson);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("errorCode").GetString().Should().Be("request.malformed");
    }
}
