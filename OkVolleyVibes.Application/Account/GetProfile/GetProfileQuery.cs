using OkVolleyVibes.Domain.Onboarding;
using OkVolleyVibes.Domain.Players;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Account.GetProfile;

/// <summary>The signed-in user's own profile + onboarding status.</summary>
public sealed record GetProfileQuery : IRequest<ProfileResponse>;

public sealed record ProfileResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string PreferredLanguage,
    DateOnly? DateOfBirth,
    IReadOnlyList<string> Roles,
    bool ProfileCompleted,
    bool HasPhoto,
    OnboardingSurveySummary? Survey);

public sealed record OnboardingSurveySummary(
    int SkillRating,
    bool HasTrainedBefore,
    string? TrainingHistory,
    IReadOnlyList<VolleyballPosition> Positions,
    RecreationalExperience? RecreationalExperience,
    ClubInterest ClubInterest,
    DateTime CompletedAtUtc);
