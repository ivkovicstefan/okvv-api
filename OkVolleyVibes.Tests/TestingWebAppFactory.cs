using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace OkVolleyVibes.Tests;

/// <summary>
/// Boots the API in the <c>Testing</c> environment for tests that do not touch the database
/// (the <c>/_diag/*</c> endpoints, error handling). A connection string is supplied so the host
/// builds, but nothing here opens a connection. DB-backed tests use their own factory.
/// </summary>
public sealed class TestingWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Seed:Enabled", "false");
        builder.UseSetting("Jwt:SigningKey", "test-signing-key-that-is-comfortably-longer-than-32-bytes");
        builder.UseSetting(
            "ConnectionStrings:Database",
            "Server=(localdb)\\MSSQLLocalDB;Database=OkVolleyVibes_Test_Unused;Trusted_Connection=True;TrustServerCertificate=True");
    }
}
