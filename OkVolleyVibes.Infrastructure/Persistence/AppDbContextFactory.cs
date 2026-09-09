using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OkVolleyVibes.Infrastructure.Persistence;

/// <summary>
/// Used only by the EF Core command-line tools (<c>dotnet ef</c>) so migrations can be generated
/// without building the API host. Runtime wiring is in <c>AddInfrastructure</c>.
/// </summary>
internal sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Same LocalDB database the app uses in Development, so `dotnet ef database update/drop`
        // acts on the database you actually run against. Override with `dotnet ef --connection` if needed.
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=OkVolleyVibes;Trusted_Connection=True;TrustServerCertificate=True",
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name))
            .Options;

        return new AppDbContext(options);
    }
}
