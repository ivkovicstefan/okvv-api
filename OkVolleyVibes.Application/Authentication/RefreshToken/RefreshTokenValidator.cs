using FluentValidation;

namespace OkVolleyVibes.Application.Authentication.RefreshToken;

internal sealed class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
