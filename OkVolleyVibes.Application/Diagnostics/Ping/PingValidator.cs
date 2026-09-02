using FluentValidation;

namespace OkVolleyVibes.Application.Diagnostics.Ping;

internal sealed class PingValidator : AbstractValidator<PingQuery>
{
    public PingValidator()
    {
        RuleFor(query => query.Message)
            .NotEmpty()
            .WithMessage("A message is required.");
    }
}
