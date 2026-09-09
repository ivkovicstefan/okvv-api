using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OkVolleyVibes.Domain.Common.Exceptions;

namespace OkVolleyVibes.Api.ExceptionHandling;

/// <summary>
/// Translates <see cref="AppException"/>s (and malformed-request errors from model binding) into
/// RFC 9457 <c>ProblemDetails</c>. Anything else is passed on to the next handler.
/// </summary>
internal sealed class AppExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<AppExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case AppException appException:
                return await WriteAsync(
                    httpContext,
                    StatusFor(appException),
                    appException.ErrorCode,
                    appException.Message,
                    appException.Errors.Count > 0 ? appException.Errors : null,
                    logLevel: LogLevel.Warning,
                    logged: appException);

            case BadHttpRequestException badRequest:
                // Bad JSON, wrong field type, unknown enum value, etc. — never a 500.
                return await WriteAsync(
                    httpContext,
                    badRequest.StatusCode is >= 400 and < 500 ? badRequest.StatusCode : StatusCodes.Status400BadRequest,
                    "request.malformed",
                    "The request body could not be read. Check the field names, types and values.",
                    errors: null,
                    logLevel: LogLevel.Warning,
                    logged: badRequest);

            default:
                return false;
        }
    }

    private async ValueTask<bool> WriteAsync(
        HttpContext httpContext,
        int status,
        string errorCode,
        string detail,
        IReadOnlyDictionary<string, string[]>? errors,
        LogLevel logLevel,
        Exception logged)
    {
        logger.Log(logLevel, logged, "Handled {ExceptionType} ({ErrorCode}) -> {StatusCode}",
            logged.GetType().Name, errorCode, status);

        httpContext.Response.StatusCode = status;

        ProblemDetails problem = new()
        {
            Status = status,
            Title = TitleFor(status),
            Detail = detail,
            Type = $"https://httpstatuses.io/{status}",
        };
        problem.Extensions["errorCode"] = errorCode;
        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
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
        UnauthorizedException => StatusCodes.Status401Unauthorized,
        NotFoundException => StatusCodes.Status404NotFound,
        ForbiddenException => StatusCodes.Status403Forbidden,
        ConflictException => StatusCodes.Status409Conflict,
        BusinessRuleException => StatusCodes.Status422UnprocessableEntity,
        _ => StatusCodes.Status400BadRequest,
    };

    private static string TitleFor(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status409Conflict => "Conflict",
        StatusCodes.Status422UnprocessableEntity => "Unprocessable Entity",
        _ => "Error",
    };
}
