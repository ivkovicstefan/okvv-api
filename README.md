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
OkVolleyVibes.Mediator/           # in-house mediator: ISender + IRequestHandler + IPipelineBehavior
OkVolleyVibes.Domain/             # entities (PlayerProfile), exceptions, Roles — zero ASP.NET/EF deps
OkVolleyVibes.Infrastructure/     # EF Core AppDbContext + migrations + ASP.NET Core Identity + adapters
OkVolleyVibes.Tests/             # xUnit + FluentAssertions + NetArchTest (LocalDB-backed integration tests)
  Architecture/ArchitectureTests  #   enforces the Clean Architecture dependency rules
```

Dependency direction: `Api → Application → Domain`, `Infrastructure → Application → Domain`,
`Application → Mediator`, nothing → `Api`. Enforced by `ArchitectureTests`. See the `dotnet-api` skill.

Request pipeline: endpoints call `ISender.Send(command)` → `LoggingBehavior` → `ValidationBehavior`
(FluentValidation → `ValidationException` on failure) → handler. See [`docs/mediator.md`](docs/mediator.md).

## Getting started

Needs SQL Server. **SQL Server LocalDB** is the zero-config default (`(localdb)\MSSQLLocalDB`);
`docker compose up -d` starts SQL Server Express as an alternative (see `docker-compose.yml`).

```bash
dotnet restore
dotnet build
dotnet test                        # integration tests spin up throwaway LocalDB databases

# run (http profile → http://localhost:5080); on Development it migrates + seeds automatically
dotnet run --project OkVolleyVibes.Api --launch-profile http

# add a migration
dotnet ef migrations add <Name> --project OkVolleyVibes.Infrastructure --startup-project OkVolleyVibes.Api --output-dir Persistence/Migrations
```

Endpoints so far:

| Route              | Purpose                          |
| ------------------ | -------------------------------- |
| `GET /health`      | Liveness probe → `200 Healthy`   |
| `GET /openapi/v1.json` | OpenAPI document (Development only) |
| `GET /swagger` | Swagger UI (Development only) |
| `POST /api/auth/register` | Sign up → `Player` role + verification email |
| `GET /api/auth/verify-email?userId=&token=` | Confirm email **and sign in** → `{ accessToken, refreshToken, … }` |
| `POST /api/auth/resend-verification` | Re-send the verification email (always `202`) |
| `POST /api/auth/refresh` · `POST /api/auth/logout` | Rotate / revoke the refresh token |
| `POST /api/account/complete-profile` | Onboarding survey → fresh tokens with `profile_completed=true` |
| `GET /api/account/profile` | The signed-in user's profile + onboarding status |
| `POST /api/account/photo` · `GET /api/account/photo/{userId}` | Profile photo (JPEG/PNG/WebP ≤ 512 KB) |
| `GET /_diag/throw/{kind}` · `/_diag/ping` · `/_diag/whoami` | Probes (Development/Testing only) |

## Accounts, identity & localization

ASP.NET Core Identity (`User`/`Role`, Guid keys) in Infrastructure; `PlayerProfile` (1:1) in Domain.
Multi-role users; self-registration → `Player`; email verification required. **JWT** access (15 min) +
rotating refresh (30 d, hashed). Verifying the email signs the user in; a **mandatory onboarding
survey** must be completed before any `ProfileComplete`-protected endpoint responds (`403 profile.incomplete`).
Localized (`en` / `sr-Latn` / `ru`) validation messages and emails.
See [`docs/identity-and-auth.md`](docs/identity-and-auth.md) and [`docs/profile-setup.md`](docs/profile-setup.md).

## Error handling

Exception-based: `AppException` categories in `OkVolleyVibes.Domain/Common/Exceptions/`
(`NotFound` → 404, `Validation` → 400, `Conflict` → 409, `BusinessRule` → 422, `Forbidden` → 403),
a chain of `IExceptionHandler`s renders RFC 9457 `ProblemDetails` with `errorCode` / `errors` /
`traceId`; unknown exceptions → generic `500`. See [`docs/error-handling.md`](docs/error-handling.md).

## Next steps (not yet done)

- Email + password **login** endpoint (issues the same JWTs; lockout enforcement) — `feature/auth-login`
- Password reset, change password, change email — their own branches
- Roles & user administration (FR-B); Google OAuth (later)
- Central Package Management (`Directory.Packages.props`); GitHub Actions CI
- Production: real email provider, Data Protection key persistence, explicit migrate step

See the functional-requirements draft and its open-questions register for what's still unscoped.
