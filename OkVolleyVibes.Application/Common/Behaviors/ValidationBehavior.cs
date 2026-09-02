using FluentValidation;
using OkVolleyVibes.Mediator;
using DomainValidationException = OkVolleyVibes.Domain.Common.Exceptions.ValidationException;

namespace OkVolleyVibes.Application.Common.Behaviors;

/// <summary>
/// Runs every registered <see cref="IValidator{T}"/> for the request. On failure it throws the
/// domain <see cref="DomainValidationException"/> (rendered as a 400 ProblemDetails by the API),
/// so handlers only ever see valid input.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IValidator<TRequest>[] _validators = validators.ToArray();

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Length == 0)
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToArray();

        if (failures.Length > 0)
        {
            var errors = failures
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray());

            throw new DomainValidationException(errors);
        }

        return await next(cancellationToken);
    }
}
