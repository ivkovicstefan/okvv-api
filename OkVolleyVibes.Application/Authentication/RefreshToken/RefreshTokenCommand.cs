using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Authentication.RefreshToken;

/// <summary>Exchanges a valid refresh token for a fresh access + refresh pair (rotating the old one).</summary>
public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthTokens>;
