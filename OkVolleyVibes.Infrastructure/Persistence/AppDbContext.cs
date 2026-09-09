using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Domain.Onboarding;
using OkVolleyVibes.Domain.Players;
using OkVolleyVibes.Infrastructure.Identity;

namespace OkVolleyVibes.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<User, Role, Guid>(options), IAppDbContext
{
    public DbSet<PlayerProfile> PlayerProfiles => Set<PlayerProfile>();

    public DbSet<OnboardingSurvey> OnboardingSurveys => Set<OnboardingSurvey>();

    public async Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await Database.BeginTransactionAsync(cancellationToken);
        return new AppDbTransaction(transaction);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
