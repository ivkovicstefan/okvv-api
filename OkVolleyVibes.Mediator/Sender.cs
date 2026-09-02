using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace OkVolleyVibes.Mediator;

/// <summary>Default <see cref="ISender"/>. Resolves the handler and behaviors from the current scope.</summary>
public sealed class Sender(IServiceProvider serviceProvider) : ISender
{
    private static readonly ConcurrentDictionary<Type, RequestHandlerWrapper> Wrappers = new();

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        RequestHandlerWrapper wrapper = Wrappers.GetOrAdd(
            request.GetType(),
            requestType =>
            {
                Type wrapperType = typeof(RequestHandlerWrapperImpl<,>)
                    .MakeGenericType(requestType, typeof(TResponse));
                return (RequestHandlerWrapper)Activator.CreateInstance(wrapperType)!;
            });

        return ((RequestHandlerWrapper<TResponse>)wrapper).Handle(request, serviceProvider, cancellationToken);
    }
}

internal abstract class RequestHandlerWrapper;

internal abstract class RequestHandlerWrapper<TResponse> : RequestHandlerWrapper
{
    public abstract Task<TResponse> Handle(
        object request, IServiceProvider services, CancellationToken cancellationToken);
}

internal sealed class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override Task<TResponse> Handle(
        object request, IServiceProvider services, CancellationToken cancellationToken)
    {
        var typedRequest = (TRequest)request;

        var handler = services.GetService<IRequestHandler<TRequest, TResponse>>()
            ?? throw new InvalidOperationException(
                $"No handler registered for request '{typeof(TRequest)}'.");

        RequestHandlerDelegate<TResponse> pipeline = ct => handler.Handle(typedRequest, ct);

        // Wrap inner-to-outer so the first-registered behavior ends up outermost.
        foreach (var behavior in services
                     .GetServices<IPipelineBehavior<TRequest, TResponse>>()
                     .Reverse())
        {
            var next = pipeline;
            var current = behavior;
            pipeline = ct => current.Handle(typedRequest, next, ct);
        }

        return pipeline(cancellationToken);
    }
}
