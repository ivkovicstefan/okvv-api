using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Domain.Identity;
using OkVolleyVibes.Domain.Players;
using OkVolleyVibes.Infrastructure.Configuration;
using OkVolleyVibes.Infrastructure.Identity;

namespace OkVolleyVibes.Infrastructure.Persistence.Seed;

/// <summary>Seeds the fixed role set always, and the configured development users when <c>Seed:Enabled</c>.</summary>
internal sealed class DatabaseSeeder(
    RoleManager<Role> roleManager,
    UserManager<User> userManager,
    AppDbContext db,
    IClock clock,
    IOptions<SeedOptions> seedOptions,
    ILogger<DatabaseSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        foreach (string roleName in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new Role(roleName) { Id = Guid.NewGuid() });
            }
        }

        SeedOptions seed = seedOptions.Value;
        if (!seed.Enabled)
        {
            return;
        }

        foreach (SeedUser seedUser in seed.Users)
        {
            if (await userManager.FindByEmailAsync(seedUser.Email) is not null)
            {
                continue;
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = seedUser.Email,
                Email = seedUser.Email,
                EmailConfirmed = true,
                FirstName = seedUser.FirstName,
                LastName = seedUser.LastName,
                PhoneNumber = seedUser.PhoneNumber,
                PreferredLanguage = "en",
                CreatedAtUtc = clock.UtcNow,
            };

            IdentityResult created = await userManager.CreateAsync(user, seedUser.Password);
            if (!created.Succeeded)
            {
                logger.LogWarning(
                    "Seed user {Email} was not created: {Errors}",
                    seedUser.Email,
                    string.Join("; ", created.Errors.Select(e => e.Description)));
                continue;
            }

            foreach (string role in seedUser.Roles)
            {
                await userManager.AddToRoleAsync(user, role);
            }

            if (seedUser.Roles.Contains(Roles.Player))
            {
                db.PlayerProfiles.Add(PlayerProfile.Create(user.Id, clock.UtcNow));
            }

            logger.LogInformation("Seeded user {Email} with roles {Roles}", seedUser.Email, string.Join(", ", seedUser.Roles));
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
