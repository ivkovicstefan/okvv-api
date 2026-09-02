namespace OkVolleyVibes.Domain.Common.Exceptions;

/// <summary>A requested resource does not exist. Maps to <c>404 Not Found</c>.</summary>
/// <remarks>
/// Use directly for one-offs (<c>new NotFoundException("Member", id)</c>) or derive a
/// concrete type when the same error is raised from several places or needs to carry data:
/// <code>
/// public sealed class MemberNotFoundException(Guid id)
///     : NotFoundException("member.not_found", $"Member '{id}' was not found.");
/// </code>
/// </remarks>
public class NotFoundException : AppException
{
    public NotFoundException(string message)
        : base("resource.not_found", message)
    {
    }

    public NotFoundException(string resource, object key)
        : base("resource.not_found", $"{resource} '{key}' was not found.")
    {
    }

    protected NotFoundException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}
