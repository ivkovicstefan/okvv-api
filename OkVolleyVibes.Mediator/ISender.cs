namespace OkVolleyVibes.Mediator;

/// <summary>Sends a request through its pipeline behaviors to its single handler.</summary>
public interface ISender
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
