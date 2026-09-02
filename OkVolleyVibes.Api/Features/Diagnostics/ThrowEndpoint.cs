using OkVolleyVibes.Api.Endpoints;
using OkVolleyVibes.Domain.Common.Exceptions;

namespace OkVolleyVibes.Api.Features.Diagnostics;

/// <summary>
/// Development/Testing only. <c>GET /_diag/throw/{kind}</c> throws a chosen exception so the
/// error-handling pipeline can be exercised by hand and in integration tests. Never mapped
/// in Staging or Production, and excluded from the OpenAPI document.
/// </summary>
public sealed class ThrowEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        IHostEnvironment env = app.ServiceProvider.GetRequiredService<IHostEnvironment>();
        if (!env.IsDevelopment() && !env.IsEnvironment("Testing"))
        {
            return;
        }

        app.MapGet("/_diag/throw/{kind}", IResult (string kind) => throw ExceptionFor(kind))
            .WithTags("Diagnostics")
            .ExcludeFromDescription();
    }

    private static Exception ExceptionFor(string kind) => kind.ToLowerInvariant() switch
    {
        "notfound" => new DiagnosticNotFoundException(),
        "validation" => new ValidationException("name", "Name is required."),
        "conflict" => new ConflictException("A widget with that name already exists."),
        "businessrule" => new BusinessRuleException("Widget is past its expiry date."),
        "forbidden" => new ForbiddenException(),
        "unexpected" => new InvalidOperationException("boom"),
        _ => new NotFoundException($"Unknown throw kind '{kind}'."),
    };
}

/// <summary>Concrete subtype, proving a derived exception is still caught by the base-type handler.</summary>
file sealed class DiagnosticNotFoundException()
    : NotFoundException("diagnostic.not_found", "Diagnostic resource was not found.");
