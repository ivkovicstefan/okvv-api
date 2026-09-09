using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Infrastructure.Configuration;
using OkVolleyVibes.Infrastructure.Email;
using OkVolleyVibes.Infrastructure.Identity;
using OkVolleyVibes.Infrastructure.Localization;
using OkVolleyVibes.Infrastructure.Persistence;
using OkVolleyVibes.Infrastructure.Persistence.Seed;
using OkVolleyVibes.Infrastructure.Time;

namespace OkVolleyVibes.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the Infrastructure layer: EF Core (<c>DbContext</c> + MSSQL), ASP.NET Core Identity,
    /// and the adapters implementing the Application's ports.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Database' is not configured.");
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name)));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        // Data Protection backs the Identity token providers (email confirmation, password reset).
        // Production should configure key persistence + encryption at rest.
        services.AddDataProtection();

        services.AddIdentityCore<User>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;

                options.Password.RequiredLength = 10;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredUniqueChars = 1;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<Role>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddPasswordValidator<CommonPasswordValidator>();

        services.Configure<AppUrls>(configuration.GetSection(AppUrls.SectionName));
        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<ITranslator, JsonTranslator>();
        services.AddScoped<IClock, SystemClock>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IEmailSender, ConsoleEmailSender>();
        services.AddScoped<IAuthLinkBuilder, AuthLinkBuilder>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
