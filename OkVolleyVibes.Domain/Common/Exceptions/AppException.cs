namespace OkVolleyVibes.Domain.Common.Exceptions;

/// <summary>
/// Base type for every error the application deliberately raises (as opposed to an
/// unexpected bug). The API layer catches this type, maps the concrete subtype to an
/// HTTP status, and renders an RFC 9457 <c>ProblemDetails</c> payload.
/// </summary>
public abstract class AppException : Exception
{
    protected AppException(string errorCode, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Stable, machine-readable identifier for this error, e.g. <c>member.not_found</c>.
    /// Clients switch on this for localized messages and UX — keep values stable.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Field-level errors keyed by property name. Empty for everything except
    /// <see cref="ValidationException"/>.
    /// </summary>
    public virtual IReadOnlyDictionary<string, string[]> Errors { get; } =
        new Dictionary<string, string[]>();
}
