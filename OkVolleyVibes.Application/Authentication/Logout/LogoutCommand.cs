using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Authentication.Logout;

/// <summary>Revokes the supplied refresh token. Idempotent — succeeds even if it's already gone.</summary>
public sealed record LogoutCommand(string RefreshToken) : IRequest<Unit>;
