using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OkVolleyVibes.Infrastructure.Persistence.Seed;

namespace OkVolleyVibes.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    /// <summary>Applies pending migrations, then runs the seeders. Call on startup in Development.</summary>
    public static async Task InitializeDatabaseAsync(
        this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(cancellationToken);

        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }
}
