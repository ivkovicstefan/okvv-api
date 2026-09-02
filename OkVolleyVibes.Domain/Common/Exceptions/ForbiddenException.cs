namespace OkVolleyVibes.Domain.Common.Exceptions;

/// <summary>
/// The caller is authenticated but not allowed to act on this resource. Maps to
/// <c>403 Forbidden</c>. For resources the caller should not even know exist, prefer
/// throwing <see cref="NotFoundException"/> instead (avoids leaking their existence).
/// </summary>
public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "You are not allowed to perform this action.")
        : base("access.forbidden", message)
    {
    }

    protected ForbiddenException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}
