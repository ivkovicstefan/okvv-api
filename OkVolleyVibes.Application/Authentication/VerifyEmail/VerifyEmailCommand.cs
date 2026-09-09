using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Authentication.VerifyEmail;

/// <summary>Confirms a user's email address from the link in the verification email.</summary>
public sealed record VerifyEmailCommand(Guid UserId, string Token) : IRequest<Unit>;
