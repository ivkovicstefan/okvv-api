using Microsoft.EntityFrameworkCore;
using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Domain.Common.Exceptions;
using OkVolleyVibes.Domain.Onboarding;
using OkVolleyVibes.Domain.Players;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Account.GetProfile;

internal sealed class GetProfileHandler(
    ICurrentUser currentUser,
    IIdentityService identity,
    IAppDbContext db,
    IUserPhotoStore photos) : IRequestHandler<GetProfileQuery, ProfileResponse>
{
    public async Task<ProfileResponse> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        Guid userId = currentUser.RequireUserId();

        UserProfileSnapshot snapshot = await identity.GetProfileAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        OnboardingSurvey? survey = await db.OnboardingSurveys
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        bool hasPhoto = await photos.ExistsAsync(userId, cancellationToken);

        OnboardingSurveySummary? summary = survey is null
            ? null
            : new OnboardingSurveySummary(
                survey.SkillRating,
                survey.HasTrainedBefore,
                survey.TrainingHistory,
                SplitPositions(survey.Positions),
                survey.RecreationalExperience,
                survey.ClubInterest,
                survey.CompletedAtUtc);

        return new ProfileResponse(
            snapshot.UserId,
            snapshot.Email,
            snapshot.FirstName,
            snapshot.LastName,
            snapshot.PhoneNumber,
            snapshot.PreferredLanguage,
            snapshot.DateOfBirth,
            snapshot.Roles,
            snapshot.ProfileCompleted,
            hasPhoto,
            summary);
    }

    private static IReadOnlyList<VolleyballPosition> SplitPositions(VolleyballPosition combined)
        => [.. Enum.GetValues<VolleyballPosition>().Where(p => p != VolleyballPosition.None && combined.HasFlag(p))];
}
