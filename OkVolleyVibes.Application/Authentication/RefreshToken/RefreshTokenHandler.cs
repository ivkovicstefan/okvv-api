using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Authentication.RefreshToken;

internal sealed class RefreshTokenHandler(ITokenService tokens)
    : IRequestHandler<RefreshTokenCommand, AuthTokens>
{
    public Task<AuthTokens> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        => tokens.RefreshAsync(request.RefreshToken, cancellationToken);
}
