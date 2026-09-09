namespace OkVolleyVibes.Domain.Common.Exceptions;

/// <summary>
/// The request needs an authenticated caller and doesn't have one. Maps to <c>401 Unauthorized</c>.
/// Usually the JWT middleware handles this; thrown explicitly only where a handler assumed a caller.
/// </summary>
public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Authentication is required.")
        : base("auth.unauthorized", message)
    {
    }

    protected UnauthorizedException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}
