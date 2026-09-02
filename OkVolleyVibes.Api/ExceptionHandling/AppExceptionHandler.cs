using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OkVolleyVibes.Domain.Common.Exceptions;

namespace OkVolleyVibes.Api.ExceptionHandling;

/// <summary>
/// Translates <see cref="AppException"/>s into RFC 9457 <c>ProblemDetails</c> responses.
/// Anything that is not an <see cref="AppException"/> is passed on to the next handler.
/// </summary>
internal sealed class AppExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<AppExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not AppException appException)
        {
            return false;
        }

        int status = StatusFor(appException);

        logger.LogWarning(
            appException,
            "Handled {ExceptionType} ({ErrorCode}) -> {StatusCode}",
            appException.GetType().Name,
            appException.ErrorCode,
            status);

        httpContext.Response.StatusCode = status;

        ProblemDetails problem = new()
        {
            Status = status,
            Title = TitleFor(status),
            Detail = appException.Message,
            Type = $"https://httpstatuses.io/{status}",
        };
        problem.Extensions["errorCode"] = appException.ErrorCode;

        if (appException.Errors.Count > 0)
        {
            problem.Extensions["errors"] = appException.Errors;
        }

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
        });
    }

    private static int StatusFor(AppException exception) => exception switch
    {
        ValidationException => StatusCodes.Status400BadRequest,
        NotFoundException => StatusCodes.Status404NotFound,
        ForbiddenException => StatusCodes.Status403Forbidden,
        ConflictException => StatusCodes.Status409Conflict,
        BusinessRuleException => StatusCodes.Status422UnprocessableEntity,
        _ => StatusCodes.Status400BadRequest,
    };

    private static string TitleFor(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status409Conflict => "Conflict",
        StatusCodes.Status422UnprocessableEntity => "Unprocessable Entity",
        _ => "Error",
    };
}
