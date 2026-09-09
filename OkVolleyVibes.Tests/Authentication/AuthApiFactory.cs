using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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
        builder.UseSetting("Jwt:SigningKey", "test-signing-key-that-is-comfortably-longer-than-32-bytes");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Email);
        });
    }

    /// <summary>Registers a user, confirms the email, and returns the caller's id + fresh tokens.</summary>
    public async Task<AuthedUser> RegisterAndVerifyAsync(string email)
    {
        HttpClient client = CreateClient();

        HttpResponseMessage register = await client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Test",
            lastName = "User",
            email,
            phoneNumber = "+381641234567",
            password = "a-good-long-passphrase-42",
        });
        register.EnsureSuccessStatusCode();
        Guid userId = (await register.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("userId").GetGuid();

        HttpResponseMessage verify = await client.GetAsync(Email.VerificationPathFor(email));
        verify.EnsureSuccessStatusCode();
        var tokens = (await verify.Content.ReadFromJsonAsync<TokenPair>())!;

        return new AuthedUser(userId, email, tokens.AccessToken, tokens.RefreshToken);
    }

    public HttpClient CreateClient(string accessToken)
    {
        HttpClient client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
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

    public sealed record AuthedUser(Guid UserId, string Email, string AccessToken, string RefreshToken);

    private sealed record TokenPair(string AccessToken, string RefreshToken);
}
