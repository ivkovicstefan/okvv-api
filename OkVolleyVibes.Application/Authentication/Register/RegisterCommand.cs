using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Authentication.Register;

/// <summary>Self-service sign-up. Creates a user with the <c>Player</c> role and sends a verification email.</summary>
public sealed record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Password) : IRequest<RegisterResponse>, ITransactionalRequest;

public sealed record RegisterResponse(Guid UserId, string Email);
