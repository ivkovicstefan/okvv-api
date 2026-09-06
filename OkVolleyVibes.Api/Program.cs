using OkVolleyVibes.Api;
using OkVolleyVibes.Application;
using OkVolleyVibes.Infrastructure;
using OkVolleyVibes.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApi();

WebApplication app = builder.Build();

app.UseApi();

if (app.Environment.IsDevelopment())
{
    await app.Services.InitializeDatabaseAsync();
}

app.Run();

// Exposed so the test project can drive the app via WebApplicationFactory<Program>.
public partial class Program;
