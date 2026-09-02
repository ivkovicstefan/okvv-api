using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OkVolleyVibes.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the Infrastructure layer: EF Core <c>DbContext</c> (MSSQL), persistence,
    /// and adapters implementing the Application's ports.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add the DbContext and port adapters here.
        // The MSSQL connection string will come from configuration.GetConnectionString("Database").
        return services;
    }
}
