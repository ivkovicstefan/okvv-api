namespace OkVolleyVibes.Domain.Common.Exceptions;

/// <summary>
/// One or more inputs failed validation. Maps to <c>400 Bad Request</c>. In normal flow this
/// is thrown once, by the validation pipeline behavior, from a set of FluentValidation failures.
/// </summary>
public sealed class ValidationException : AppException
{
    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("validation.failed", "One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public ValidationException(string field, string error)
        : this(new Dictionary<string, string[]> { [field] = [error] })
    {
    }

    public override IReadOnlyDictionary<string, string[]> Errors { get; }
}
