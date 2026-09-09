namespace OkVolleyVibes.Application.Common.Abstractions;

/// <summary>The authenticated caller for the current request, read from the validated access token.</summary>
public interface ICurrentUser
{
    Guid? UserId { get; }

    bool IsAuthenticated { get; }

    bool ProfileCompleted { get; }

    IReadOnlyCollection<string> Roles { get; }

    /// <summary>The caller's id, or throws if the request is not authenticated.</summary>
    Guid RequireUserId();
}
