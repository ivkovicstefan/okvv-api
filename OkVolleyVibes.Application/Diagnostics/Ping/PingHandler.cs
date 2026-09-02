using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Diagnostics.Ping;

internal sealed class PingHandler : IRequestHandler<PingQuery, PingResponse>
{
    public Task<PingResponse> Handle(PingQuery request, CancellationToken cancellationToken)
        => Task.FromResult(new PingResponse($"pong: {request.Message}"));
}
