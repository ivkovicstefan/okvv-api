using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace OkVolleyVibes.Api.ExceptionHandling;

/// <summary>
/// Last-resort handler for anything that is not a deliberate <c>AppException</c>: logs the
/// full exception at Error and returns a generic <c>500</c> without leaking internals.
/// </summary>
internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        ProblemDetails problem = new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred.",
            Type = "https://httpstatuses.io/500",
        };
        problem.Extensions["errorCode"] = "server.unexpected";

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
        });
    }
}
