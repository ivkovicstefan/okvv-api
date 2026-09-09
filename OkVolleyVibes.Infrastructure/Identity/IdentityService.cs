using Microsoft.AspNetCore.Identity;
using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Domain.Common.Exceptions;
using OkVolleyVibes.Domain.Identity;

namespace OkVolleyVibes.Infrastructure.Identity;

internal sealed class IdentityService(UserManager<User> userManager, IClock clock) : IIdentityService
{
    public async Task<Guid?> FindUserIdByEmailAsync(string email, CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByEmailAsync(email);
        return user?.Id;
    }

    public async Task<Guid> CreateUserAsync(NewUser data, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = data.Email,
            Email = data.Email,
            FirstName = data.FirstName,
            LastName = data.LastName,
            PhoneNumber = data.PhoneNumber,
            PreferredLanguage = data.PreferredLanguage,
            CreatedAtUtc = clock.UtcNow,
        };

        IdentityResult result = await userManager.CreateAsync(user, data.Password);
        if (result.Succeeded)
        {
            return user.Id;
        }

        if (result.Errors.Any(e => e.Code is "DuplicateUserName" or "DuplicateEmail"))
        {
            throw new EmailAlreadyInUseException();
        }

        string[] passwordErrors = result.Errors
            .Where(e => e.Code.StartsWith("Password", StringComparison.Ordinal))
            .Select(e => e.Description)
            .ToArray();

        if (passwordErrors.Length > 0)
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["password"] = passwordErrors });
        }

        throw new ValidationException(new Dictionary<string, string[]>
        {
            ["registration"] = result.Errors.Select(e => e.Description).ToArray(),
        });
    }

    public async Task AddToRoleAsync(Guid userId, string role, CancellationToken cancellationToken)
    {
        User user = await RequireUser(userId);
        IdentityResult result = await userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["role"] = result.Errors.Select(e => e.Description).ToArray(),
            });
        }
    }

    public async Task<bool> IsEmailConfirmedAsync(Guid userId, CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByIdAsync(userId.ToString());
        return user is not null && await userManager.IsEmailConfirmedAsync(user);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        User user = await RequireUser(userId);
        return await userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken)
    {
        User user = await RequireUser(userId);

        IdentityResult result = await userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            throw new ValidationException("token", "The confirmation link is invalid or has expired.");
        }
    }

    public async Task<UserProfileSnapshot?> GetProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return null;
        }

        IList<string> roles = await userManager.GetRolesAsync(user);

        return new UserProfileSnapshot(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.PreferredLanguage,
            user.DateOfBirth,
            user.ProfileCompletedAtUtc is not null,
            [.. roles]);
    }

    public async Task UpdateProfileBasicsAsync(
        Guid userId, DateOnly dateOfBirth, string preferredLanguage, CancellationToken cancellationToken)
    {
        User user = await RequireUser(userId);
        user.DateOfBirth = dateOfBirth;
        user.PreferredLanguage = preferredLanguage;

        IdentityResult result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["profile"] = result.Errors.Select(e => e.Description).ToArray(),
            });
        }
    }

    public async Task MarkOnboardingCompletedAsync(Guid userId, DateTime completedAtUtc, CancellationToken cancellationToken)
    {
        User user = await RequireUser(userId);
        user.ProfileCompletedAtUtc = completedAtUtc;
        await userManager.UpdateAsync(user);
    }

    private async Task<User> RequireUser(Guid userId)
        => await userManager.FindByIdAsync(userId.ToString())
           ?? throw new NotFoundException("User", userId);
}
