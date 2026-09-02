namespace OkVolleyVibes.Domain.Common.Exceptions;

/// <summary>
/// The request conflicts with the current state of a resource (uniqueness, concurrency,
/// already-exists). Maps to <c>409 Conflict</c>.
/// </summary>
public class ConflictException : AppException
{
    public ConflictException(string message)
        : base("resource.conflict", message)
    {
    }

    protected ConflictException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}
