using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Authentication.VerifyEmail;

/// <summary>
/// Confirms a user's email from the verification link and, on success, signs them in
/// (returns an access + refresh pair) so they can go straight into profile setup.
/// </summary>
public sealed record VerifyEmailCommand(Guid UserId, string Token) : IRequest<AuthTokens>;
