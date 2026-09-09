using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Authentication.VerifyEmail;

internal sealed class VerifyEmailHandler(IIdentityService identity)
    : IRequestHandler<VerifyEmailCommand, Unit>
{
    public async Task<Unit> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        await identity.ConfirmEmailAsync(request.UserId, request.Token, cancellationToken);
        return Unit.Value;
    }
}
