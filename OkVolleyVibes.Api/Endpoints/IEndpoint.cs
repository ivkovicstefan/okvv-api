namespace OkVolleyVibes.Api.Endpoints;

/// <summary>
/// A single HTTP endpoint following the REPR pattern (Request → Endpoint → Response).
/// One implementation per file. Implementations are discovered by assembly scan and
/// mapped at startup (see <see cref="EndpointExtensions"/>).
/// </summary>
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
