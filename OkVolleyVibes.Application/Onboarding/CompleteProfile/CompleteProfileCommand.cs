using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Domain.Onboarding;
using OkVolleyVibes.Domain.Players;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Onboarding.CompleteProfile;

/// <summary>
/// The onboarding survey (FR-A11). On success it stamps the user as onboarded and returns a
/// fresh token pair carrying <c>profile_completed=true</c>.
/// </summary>
public sealed record CompleteProfileCommand(
    DateOnly DateOfBirth,
    string PreferredLanguage,
    int SkillRating,
    bool HasTrainedBefore,
    string? TrainingHistory,
    IReadOnlyList<VolleyballPosition> Positions,
    RecreationalExperience? RecreationalExperience,
    ClubInterest ClubInterest,
    bool AgreedToClubRules) : IRequest<AuthTokens>, ITransactionalRequest;
