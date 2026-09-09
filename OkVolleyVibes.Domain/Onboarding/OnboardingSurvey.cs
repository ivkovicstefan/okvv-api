using OkVolleyVibes.Domain.Players;

namespace OkVolleyVibes.Domain.Onboarding;

/// <summary>
/// The one-time onboarding questionnaire, completed right after email verification (FR-A11).
/// One per user (1:1) by <see cref="UserId"/>.
/// </summary>
public sealed class OnboardingSurvey
{
    private OnboardingSurvey()
    {
        // EF Core
    }

    private OnboardingSurvey(
        Guid userId,
        int skillRating,
        bool hasTrainedBefore,
        string? trainingHistory,
        VolleyballPosition positions,
        RecreationalExperience? recreationalExperience,
        ClubInterest clubInterest,
        bool agreedToClubRules,
        DateTime completedAtUtc)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        SkillRating = skillRating;
        HasTrainedBefore = hasTrainedBefore;
        TrainingHistory = trainingHistory;
        Positions = positions;
        RecreationalExperience = recreationalExperience;
        ClubInterest = clubInterest;
        AgreedToClubRules = agreedToClubRules;
        CompletedAtUtc = completedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    /// <summary>Self-rated volleyball skill, 1&ndash;10.</summary>
    public int SkillRating { get; private set; }

    public bool HasTrainedBefore { get; private set; }

    /// <summary>Free text: clubs and periods of training / playing. Set when <see cref="HasTrainedBefore"/>.</summary>
    public string? TrainingHistory { get; private set; }

    /// <summary>One or more positions; <see cref="VolleyballPosition.None"/> when never formally trained.</summary>
    public VolleyballPosition Positions { get; private set; }

    /// <summary>Set when NOT <see cref="HasTrainedBefore"/>.</summary>
    public RecreationalExperience? RecreationalExperience { get; private set; }

    public ClubInterest ClubInterest { get; private set; }

    public bool AgreedToClubRules { get; private set; }

    public DateTime CompletedAtUtc { get; private set; }

    public static OnboardingSurvey Create(
        Guid userId,
        int skillRating,
        bool hasTrainedBefore,
        string? trainingHistory,
        VolleyballPosition positions,
        RecreationalExperience? recreationalExperience,
        ClubInterest clubInterest,
        bool agreedToClubRules,
        DateTime completedAtUtc)
        => new(
            userId,
            skillRating,
            hasTrainedBefore,
            trainingHistory,
            positions,
            recreationalExperience,
            clubInterest,
            agreedToClubRules,
            completedAtUtc);
}
