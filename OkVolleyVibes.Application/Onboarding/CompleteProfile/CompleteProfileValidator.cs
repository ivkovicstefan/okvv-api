using FluentValidation;
using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Domain.Common;
using OkVolleyVibes.Domain.Players;

namespace OkVolleyVibes.Application.Onboarding.CompleteProfile;

internal sealed class CompleteProfileValidator : AbstractValidator<CompleteProfileCommand>
{
    public CompleteProfileValidator(ITranslator t, IClock clock)
    {
        RuleFor(x => x.DateOfBirth)
            .Must(dob => IsPlausibleAge(dob, clock.UtcNow))
            .WithMessage(t["Profile.Dob.Implausible"]);

        RuleFor(x => x.PreferredLanguage)
            .Must(Language.IsSupported)
            .WithMessage(t["Profile.Language.Unsupported"]);

        RuleFor(x => x.SkillRating)
            .InclusiveBetween(1, 10)
            .WithMessage(t["Profile.Skill.Range"]);

        When(x => x.HasTrainedBefore, () =>
        {
            RuleFor(x => x.TrainingHistory)
                .NotEmpty().WithMessage(t["Profile.TrainingHistory.Required"])
                .MaximumLength(2000);

            RuleFor(x => x.Positions)
                .NotEmpty().WithMessage(t["Profile.Positions.Required"]);

            RuleForEach(x => x.Positions)
                .Must(p => p != VolleyballPosition.None && Enum.IsDefined(p))
                .WithMessage(t["Profile.Positions.Invalid"]);
        });

        When(x => !x.HasTrainedBefore, () =>
        {
            RuleFor(x => x.RecreationalExperience)
                .NotNull().WithMessage(t["Profile.RecExperience.Required"])
                .IsInEnum();
        });

        RuleFor(x => x.ClubInterest).IsInEnum();

        RuleFor(x => x.AgreedToClubRules)
            .Equal(true).WithMessage(t["Profile.Rules.MustAgree"]);
    }

    private static bool IsPlausibleAge(DateOnly dateOfBirth, DateTime nowUtc)
    {
        DateOnly today = DateOnly.FromDateTime(nowUtc);
        if (dateOfBirth > today)
        {
            return false;
        }

        int age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age))
        {
            age--;
        }

        return age is >= 5 and <= 100;
    }
}
