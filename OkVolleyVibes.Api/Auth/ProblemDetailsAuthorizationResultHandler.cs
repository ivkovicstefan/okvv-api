using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;
using OkVolleyVibes.Application.Common.Abstractions;

namespace OkVolleyVibes.Api.Auth;

/// <summary>
/// Renders authorization failures as RFC 9457 <c>ProblemDetails</c> with a stable <c>errorCode</c>:
/// <c>auth.unauthorized</c> (401), <c>profile.incomplete</c> (403, onboarding not finished),
/// or <c>access.forbidden</c> (403).
/// </summary>
internal sealed class ProblemDetailsAuthorizationResultHandler(IProblemDetailsService problemDetailsService)
    : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Succeeded)
        {
            await _default.HandleAsync(next, context, policy, authorizeResult);
            return;
        }

        bool authenticated = context.User.Identity?.IsAuthenticated ?? false;

        (int status, string code, string title, string detail) = !authenticated
            ? (StatusCodes.Status401Unauthorized, "auth.unauthorized", "Unauthorized", "Authentication is required.")
            : FailedTheProfileRequirement(authorizeResult)
                ? (StatusCodes.Status403Forbidden, "profile.incomplete", "Forbidden", "Finish setting up your profile before continuing.")
                : (StatusCodes.Status403Forbidden, "access.forbidden", "Forbidden", "You are not allowed to perform this action.");

        context.Response.StatusCode = status;

        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Type = $"https://httpstatuses.io/{status}",
                Extensions = { ["errorCode"] = code },
            },
        });
    }

    private static bool FailedTheProfileRequirement(PolicyAuthorizationResult result)
        => result.AuthorizationFailure?.FailedRequirements
               .OfType<ClaimsAuthorizationRequirement>()
               .Any(requirement => requirement.ClaimType == AppClaimTypes.ProfileCompleted)
           ?? false;
}
