using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Common.Behaviors;

/// <summary>
/// Wraps requests marked <see cref="ITransactionalRequest"/> in one database transaction:
/// commit on success, roll back (via dispose) on any exception. Other requests pass straight through.
/// </summary>
public sealed class TransactionBehavior<TRequest, TResponse>(IAppDbContext db)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ITransactionalRequest)
        {
            return await next(cancellationToken);
        }

        await using IAppTransaction transaction = await db.BeginTransactionAsync(cancellationToken);

        TResponse response = await next(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return response;
    }
}
