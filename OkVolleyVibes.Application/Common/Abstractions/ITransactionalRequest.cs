namespace OkVolleyVibes.Application.Common.Abstractions;

/// <summary>
/// Marks a request that must run inside a single database transaction. <c>TransactionBehavior</c>
/// opens the transaction before the handler and commits only if it completes without throwing.
/// Put it on commands that make more than one write (e.g. create user + assign role + create profile).
/// </summary>
public interface ITransactionalRequest;
