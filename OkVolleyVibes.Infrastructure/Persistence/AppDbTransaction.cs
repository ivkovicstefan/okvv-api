using Microsoft.EntityFrameworkCore.Storage;
using OkVolleyVibes.Application.Common.Abstractions;

namespace OkVolleyVibes.Infrastructure.Persistence;

internal sealed class AppDbTransaction(IDbContextTransaction transaction) : IAppTransaction
{
    public Task CommitAsync(CancellationToken cancellationToken = default)
        => transaction.CommitAsync(cancellationToken);

    public ValueTask DisposeAsync()
        => transaction.DisposeAsync();
}
