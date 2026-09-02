using OkVolleyVibes.Api;
using OkVolleyVibes.Application;
using OkVolleyVibes.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApi();

WebApplication app = builder.Build();

app.UseApi();

app.Run();

// Exposed so the test project can drive the app via WebApplicationFactory<Program>.
public partial class Program;
