# OK VV — API

Backend API for **OK Volley Vibes**.

- **Stack:** ASP.NET Core .NET 10 (LTS), C# 14, EF Core 10, ASP.NET Core Identity + JWT, MSSQL (SQL Server Express in Docker, or Azure SQL free serverless offer).
- **Architecture:** Clean Architecture + REPR minimal-API endpoints + use-case ("case") folders in the Application layer.
- **Serves:** `okvv-web` (httpOnly cookie) and `okvv-mobile` (bearer token). Publishes an OpenAPI document that both frontends generate typed clients from.

## Solution layout

```
OkVolleyVibes.slnx                 # XML solution (src/ + tests/ solution folders)
Directory.Build.props              # shared: net10.0, nullable, warnings-as-errors
global.json                        # pins SDK 10.0.400

OkVolleyVibes.Api/                 # minimal-API host
  Program.cs                       #   builder → AddApplication/AddInfrastructure/AddApi → UseApi → Run
  DependencyInjection.cs           #   AddApi() / UseApi()
  Endpoints/                       #   IEndpoint + assembly-scan registration (REPR infrastructure)
  Features/<Feature>/*Endpoint.cs  #   one REPR endpoint per file  (Features/Health = reference example)
OkVolleyVibes.Application/         # use cases, ports, validators, pipeline behaviors  (AddApplication())
OkVolleyVibes.Domain/             # entities, value objects, domain events — zero dependencies
OkVolleyVibes.Infrastructure/     # EF Core DbContext + migrations + port adapters  (AddInfrastructure())
OkVolleyVibes.Tests/             # xUnit + FluentAssertions + NetArchTest
  Endpoints/HealthEndpointTests   #   /health integration test via WebApplicationFactory
  Architecture/ArchitectureTests  #   enforces the Clean Architecture dependency rules
```

Dependency direction: `Api → Application → Domain`, `Infrastructure → Application → Domain`, nothing → `Api`.
Enforced by `ArchitectureTests`. See the `dotnet-api` skill for conventions.

## Getting started

```bash
dotnet restore
dotnet build
dotnet test

# run (http profile → http://localhost:5080, https → https://localhost:7080)
dotnet run --project OkVolleyVibes.Api --launch-profile http
```

Endpoints so far:

| Route              | Purpose                          |
| ------------------ | -------------------------------- |
| `GET /health`      | Liveness probe → `200 Healthy`   |
| `GET /openapi/v1.json` | OpenAPI document (Development only) |

## Next steps (not yet done)

- EF Core `AppDbContext` + `IAppDbContext` port + first migration (MSSQL)
- Request dispatcher (`ISender`) + validation pipeline behavior + `ProblemDetails` mapping
- ASP.NET Core Identity + JWT, authorization policies (CEO / Coach / Player + team-scoped claims)
- Central Package Management (`Directory.Packages.props`)
- Dockerfile + `docker compose` (API + SQL Server Express) + GitHub Actions CI
