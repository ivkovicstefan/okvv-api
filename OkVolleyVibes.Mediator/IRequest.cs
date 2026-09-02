namespace OkVolleyVibes.Mediator;

/// <summary>Marker for a request that yields a <typeparamref name="TResponse"/> when sent through <see cref="ISender"/>.</summary>
public interface IRequest<out TResponse>;

/// <summary>Response placeholder for requests that produce no meaningful value.</summary>
public readonly record struct Unit
{
    public static readonly Unit Value = default;
}
