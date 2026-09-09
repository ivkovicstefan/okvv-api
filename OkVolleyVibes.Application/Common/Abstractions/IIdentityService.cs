namespace OkVolleyVibes.Application.Common.Abstractions;

/// <summary>
/// The Application layer's window onto ASP.NET Core Identity (which lives in Infrastructure).
/// Implementations translate Identity failures into the domain exceptions
/// (<c>ConflictException</c>, <c>ValidationException</c>, <c>NotFoundException</c>).
/// </summary>
public interface IIdentityService
{
    Task<Guid?> FindUserIdByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>Creates a user. Throws <c>ConflictException</c> if the email is taken, <c>ValidationException</c> if the password is rejected.</summary>
    Task<Guid> CreateUserAsync(NewUser user, CancellationToken cancellationToken);

    Task AddToRoleAsync(Guid userId, string role, CancellationToken cancellationToken);

    Task<bool> IsEmailConfirmedAsync(Guid userId, CancellationToken cancellationToken);

    Task<string> GenerateEmailConfirmationTokenAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Confirms the address. Throws <c>NotFoundException</c> for an unknown user, <c>ValidationException</c> for a bad or expired token.</summary>
    Task ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken);

    Task<UserProfileSnapshot?> GetProfileAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Updates the basic profile fields collected during onboarding / in profile settings.</summary>
    Task UpdateProfileBasicsAsync(
        Guid userId, DateOnly dateOfBirth, string preferredLanguage, CancellationToken cancellationToken);

    /// <summary>Stamps the user as having finished onboarding.</summary>
    Task MarkOnboardingCompletedAsync(Guid userId, DateTime completedAtUtc, CancellationToken cancellationToken);
}

/// <summary>Data needed to create a new account.</summary>
public sealed record NewUser(
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string Password,
    string PreferredLanguage);

/// <summary>Read-only view of a user's identity + basic profile.</summary>
public sealed record UserProfileSnapshot(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string PreferredLanguage,
    DateOnly? DateOfBirth,
    bool ProfileCompleted,
    IReadOnlyList<string> Roles);
