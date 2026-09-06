using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Authentication.ResendVerification;

/// <summary>Re-sends the verification email. Responds the same whether or not the account exists.</summary>
public sealed record ResendVerificationCommand(string Email) : IRequest<Unit>;
