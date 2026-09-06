namespace OkVolleyVibes.Application.Common.Abstractions;

/// <summary>The current time, as a port so handlers stay testable.</summary>
public interface IClock
{
    DateTime UtcNow { get; }
}
