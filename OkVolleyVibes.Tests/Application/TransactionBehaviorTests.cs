using Microsoft.EntityFrameworkCore;
using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Application.Common.Behaviors;
using OkVolleyVibes.Domain.Onboarding;
using OkVolleyVibes.Domain.Players;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Tests.Application;

public sealed class TransactionBehaviorTests
{
    [Fact]
    public async Task Commits_and_disposes_after_a_successful_transactional_request()
    {
        var db = new FakeDb();
        var behavior = new TransactionBehavior<TxCommand, string>(db);

        string result = await behavior.Handle(new TxCommand(), _ => Task.FromResult("ok"), CancellationToken.None);

        result.Should().Be("ok");
        db.Transaction!.Committed.Should().BeTrue();
        db.Transaction.Disposed.Should().BeTrue();
    }

    [Fact]
    public async Task Rolls_back_when_the_handler_throws()
    {
        var db = new FakeDb();
        var behavior = new TransactionBehavior<TxCommand, string>(db);

        Func<Task> act = () => behavior.Handle(
            new TxCommand(),
            _ => throw new InvalidOperationException("boom"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        db.Transaction!.Committed.Should().BeFalse();
        db.Transaction.Disposed.Should().BeTrue();
    }

    [Fact]
    public async Task Does_not_open_a_transaction_for_a_non_transactional_request()
    {
        var db = new FakeDb();
        var behavior = new TransactionBehavior<PlainQuery, string>(db);

        await behavior.Handle(new PlainQuery(), _ => Task.FromResult("ok"), CancellationToken.None);

        db.OpenedTransaction.Should().BeFalse();
    }

    private sealed record TxCommand : IRequest<string>, ITransactionalRequest;

    private sealed record PlainQuery : IRequest<string>;

    private sealed class FakeDb : IAppDbContext
    {
        public bool OpenedTransaction { get; private set; }

        public FakeTransaction? Transaction { get; private set; }

        public DbSet<PlayerProfile> PlayerProfiles => throw new NotSupportedException();

        public DbSet<OnboardingSurvey> OnboardingSurveys => throw new NotSupportedException();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);

        public Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            OpenedTransaction = true;
            Transaction = new FakeTransaction();
            return Task.FromResult<IAppTransaction>(Transaction);
        }
    }

    private sealed class FakeTransaction : IAppTransaction
    {
        public bool Committed { get; private set; }

        public bool Disposed { get; private set; }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            Committed = true;
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            Disposed = true;
            return ValueTask.CompletedTask;
        }
    }
}
