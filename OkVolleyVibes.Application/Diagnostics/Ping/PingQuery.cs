using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Diagnostics.Ping;

/// <summary>Diagnostic request that round-trips a message through the full mediator pipeline.</summary>
public sealed record PingQuery(string? Message) : IRequest<PingResponse>;

public sealed record PingResponse(string Reply);
