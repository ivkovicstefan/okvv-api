using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Authentication.Logout;

internal sealed class LogoutHandler(ITokenService tokens) : IRequestHandler<LogoutCommand, Unit>
{
    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await tokens.RevokeAsync(request.RefreshToken, cancellationToken);
        return Unit.Value;
    }
}
