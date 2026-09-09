using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Infrastructure.Persistence;

namespace OkVolleyVibes.Tests.Authentication;

/// <summary>
/// Boots the API against a throwaway LocalDB database and swaps in a capturing email sender.
/// One database per factory instance; created on init, dropped on dispose.
/// </summary>
public sealed class AuthApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _database = $"OkVolleyVibes_Test_{Guid.NewGuid():N}";

    public FakeEmailSender Email { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting(
            "ConnectionStrings:Database",
            $"Server=(localdb)\\MSSQLLocalDB;Database={_database};Trusted_Connection=True;TrustServerCertificate=True");
        builder.UseSetting("Seed:Enabled", "false");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Email);
        });
    }

    async Task IAsyncLifetime.InitializeAsync()
        => await Services.InitializeDatabaseAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        using (IServiceScope scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
        }

        await base.DisposeAsync();
    }
}
