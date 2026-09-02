using Microsoft.Extensions.Logging;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Common.Behaviors;

/// <summary>Logs the start, completion, and failure of every request that passes through the pipeline.</summary>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        logger.LogInformation("Handling {Request}", requestName);

        try
        {
            TResponse response = await next(cancellationToken);
            logger.LogInformation("Handled {Request}", requestName);
            return response;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "{Request} failed", requestName);
            throw;
        }
    }
}
