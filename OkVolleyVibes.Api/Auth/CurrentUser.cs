using System.Security.Claims;
using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Domain.Common.Exceptions;

namespace OkVolleyVibes.Api.Auth;

internal sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId =>
        Guid.TryParse(Principal?.FindFirstValue(AppClaimTypes.Subject), out Guid id) ? id : null;

    public bool ProfileCompleted => string.Equals(
        Principal?.FindFirstValue(AppClaimTypes.ProfileCompleted),
        "true",
        StringComparison.OrdinalIgnoreCase);

    public IReadOnlyCollection<string> Roles =>
        Principal?.FindAll(AppClaimTypes.Role).Select(claim => claim.Value).ToArray() ?? [];

    public Guid RequireUserId()
        => UserId ?? throw new UnauthorizedException("You must be signed in.");
}
