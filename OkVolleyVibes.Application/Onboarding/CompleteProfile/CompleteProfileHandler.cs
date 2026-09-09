using Microsoft.EntityFrameworkCore;
using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Domain.Common.Exceptions;
using OkVolleyVibes.Domain.Onboarding;
using OkVolleyVibes.Domain.Players;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Onboarding.CompleteProfile;

internal sealed class CompleteProfileHandler(
    ICurrentUser currentUser,
    IIdentityService identity,
    IAppDbContext db,
    ITokenService tokens,
    IClock clock) : IRequestHandler<CompleteProfileCommand, AuthTokens>
{
    public async Task<AuthTokens> Handle(CompleteProfileCommand request, CancellationToken cancellationToken)
    {
        Guid userId = currentUser.RequireUserId();

        if (await db.OnboardingSurveys.AnyAsync(s => s.UserId == userId, cancellationToken))
        {
            throw new ConflictException("Your profile has already been set up.");
        }

        await identity.UpdateProfileBasicsAsync(
            userId, request.DateOfBirth, request.PreferredLanguage, cancellationToken);

        VolleyballPosition positions = request.HasTrainedBefore
            ? request.Positions.Aggregate(VolleyballPosition.None, (acc, p) => acc | p)
            : VolleyballPosition.None;

        db.OnboardingSurveys.Add(OnboardingSurvey.Create(
            userId,
            request.SkillRating,
            request.HasTrainedBefore,
            request.HasTrainedBefore ? request.TrainingHistory?.Trim() : null,
            positions,
            request.HasTrainedBefore ? null : request.RecreationalExperience,
            request.ClubInterest,
            request.AgreedToClubRules,
            clock.UtcNow));

        await identity.MarkOnboardingCompletedAsync(userId, clock.UtcNow, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return await tokens.IssueAsync(userId, cancellationToken);
    }
}
