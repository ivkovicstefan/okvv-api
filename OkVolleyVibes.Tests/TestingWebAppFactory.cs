using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace OkVolleyVibes.Tests;

/// <summary>Boots the API in the <c>Testing</c> environment (enables the <c>/_diag/*</c> endpoints).</summary>
public sealed class TestingWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder.UseEnvironment("Testing");
}
