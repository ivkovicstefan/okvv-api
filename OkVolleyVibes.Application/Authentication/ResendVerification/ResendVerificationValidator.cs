using FluentValidation;

namespace OkVolleyVibes.Application.Authentication.ResendVerification;

internal sealed class ResendVerificationValidator : AbstractValidator<ResendVerificationCommand>
{
    public ResendVerificationValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
