namespace OkVolleyVibes.Mediator;

/// <summary>Invokes the next step in the pipeline (the following behavior, or finally the handler).</summary>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken cancellationToken);

/// <summary>
/// Wraps request handling. Behaviors run in registration order (first registered = outermost);
/// each decides whether and when to call <paramref name="next"/>.
/// </summary>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}
