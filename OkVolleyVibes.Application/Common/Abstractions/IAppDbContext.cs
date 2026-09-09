using Microsoft.EntityFrameworkCore;
using OkVolleyVibes.Domain.Onboarding;
using OkVolleyVibes.Domain.Players;

namespace OkVolleyVibes.Application.Common.Abstractions;

/// <summary>
/// The application's write model. Handlers depend on this, not on the concrete <c>DbContext</c>.
/// The <c>DbSet&lt;T&gt;</c> surface is the one EF Core type the Application layer is allowed to see.
/// </summary>
public interface IAppDbContext
{
    DbSet<PlayerProfile> PlayerProfiles { get; }

    DbSet<OnboardingSurvey> OnboardingSurveys { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Starts a database transaction. Used by <c>TransactionBehavior</c> for commands.</summary>
    Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

/// <summary>A database transaction; disposing without committing rolls back.</summary>
public interface IAppTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
