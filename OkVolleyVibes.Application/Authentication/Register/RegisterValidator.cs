using FluentValidation;
using OkVolleyVibes.Application.Common.Abstractions;

namespace OkVolleyVibes.Application.Authentication.Register;

internal sealed class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator(ITranslator t)
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(t["Register.FirstName.Required"])
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(t["Register.LastName.Required"])
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(t["Register.Email.Required"])
            .EmailAddress().WithMessage(t["Register.Email.Invalid"])
            .MaximumLength(256);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage(t["Register.Phone.Required"])
            .Matches(@"^\+?[0-9 ().\-]{6,20}$").WithMessage(t["Register.Phone.Invalid"]);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(t["Register.Password.Required"])
            .MinimumLength(10).WithMessage(t.Format("Register.Password.TooShort", 10));
    }
}
