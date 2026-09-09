# Identity, accounts & localization

Covers `feature/identity-and-registration`: the persistence + ASP.NET Core Identity foundation,
the request-localization infrastructure, and the registration / email-verification flow (FR-A1–A2,
A9). Login, password reset and email change follow in later branches.

## Where things live

| Concern | Location | Notes |
| --- | --- | --- |
| `User : IdentityUser<Guid>`, `Role : IdentityRole<Guid>` | `Infrastructure/Identity/` | Identity is an infrastructure concern. `User` adds `FirstName`, `LastName`, `PreferredLanguage`, `CreatedAtUtc`. |
| `PlayerProfile` (domain entity) | `Domain/Players/` | 1:1 with a user by `Guid UserId`; **no** navigation to `User`. Holds `MembershipStatus` (starts `Pending`); the rest arrives with FR-C. |
| `AppDbContext : IdentityDbContext<User, Role, Guid>, IAppDbContext` | `Infrastructure/Persistence/` | The one write model. `IAppDbContext` (Application) exposes `DbSet<PlayerProfile>` + `SaveChangesAsync` + `BeginTransactionAsync`. |
| `IIdentityService` | port in `Application/Common/Abstractions/`, impl in `Infrastructure/Identity/` | Application's only window onto Identity. Maps Identity failures to domain exceptions (`EmailAlreadyInUseException` → 409, password/other → `ValidationException` → 400, unknown user → 404). |
| `IEmailSender`, `IClock`, `IAuthLinkBuilder`, `ITranslator` | ports in Application, adapters in Infrastructure | Dev email sender writes to the log. |

`Infrastructure` carries `<FrameworkReference Include="Microsoft.AspNetCore.App" />` (Identity, Data
Protection). `Domain` and `Application` stay off ASP.NET — enforced by architecture tests.

## Identity configuration (the "Recommended" security set)

- Password: min **10** chars, no character-class rules, **plus** an embedded common/breached-password
  blocklist (`Infrastructure/Identity/common-passwords.txt`, `CommonPasswordValidator`). An online
  HaveIBeenPwned check can be layered on later.
- Lockout: **5** failed attempts → **15 min**.
- `SignIn.RequireConfirmedEmail = true` — unverified accounts cannot sign in (login lands next branch).
- Email-confirmation token: DataProtector-based, default **24 h** lifespan.
- Unique email required.

## Transactions

`TransactionBehavior` (mediator pipeline) wraps any request marked `ITransactionalRequest` in one
DB transaction: commit on success, roll back on exception. `RegisterCommand` uses it so
*create user → assign Player role → create PlayerProfile* is atomic.

## Localization

- Supported UI cultures: **`en`** (default), **`sr-Latn`**, **`ru`**. `AddRequestLocalization`
  negotiates from `Accept-Language` (an authenticated user's `PreferredLanguage` will feed in once
  login exists).
- Server strings (validation messages, email subject/body) come from `ITranslator`, backed by
  embedded JSON: `Infrastructure/Localization/strings-{culture}.json`. Lookup order: current UI
  culture → its parent → `en` → the key itself. Files are named with a hyphen (`strings-en.json`)
  so MSBuild does not treat the culture segment as a satellite resource.
- Clients still localize their own UI by the stable `errorCode`; the server only localizes text it
  originates.

## Seeding

The **`SeedBootstrapAccounts` migration** inserts the five roles (`CEO`, `FinanceManager`, `Coach`,
`RecreationCoordinator`, `Player`) and the three founding club accounts — Stefan (CEO, Coach,
RecreationCoordinator, Player), Luka (Coach, Player), Aleksa (RecreationCoordinator, Player) — with
their role assignments and player profiles. Fixed GUIDs; a **fixed hash of the development password
`DevPassw0rd!42`**. It is a dev/staging bootstrap — **do not apply it to real production** (provision
production accounts with per-user passwords or a reset flow instead).

`DatabaseSeeder` remains for *extra* ad-hoc dev users: set `Seed:Enabled: true` and add entries to
`Seed:Users` in `appsettings.Development.json` (its role loop is now a no-op — the migration owns the
roles). `DatabaseInitializer.InitializeDatabaseAsync` (migrate + seed) runs on startup in Development.
There is no "add a person" feature; everyone else self-registers.

`AppDbContextFactory` points the EF CLI at the same `OkVolleyVibes` LocalDB the app uses, so
`dotnet ef database update` / `drop` act on the database you run against.

## Endpoints

| Route | Purpose |
| --- | --- |
| `POST /api/auth/register` | Create account (→ `Player`), send verification email. `201` + `{ userId, email }`; `409` `account.email_in_use`; `400` validation. |
| `GET /api/auth/verify-email?userId=&token=` | Confirm the address. `204`; `400` bad token; `404` unknown user. |
| `POST /api/auth/resend-verification` | Re-send. Always `202`, whether or not the account exists. |

All three are behind the `auth` rate-limit policy (fixed window, 10 / 5 min per IP).

## Database

LocalDB is the default (`Server=(localdb)\MSSQLLocalDB;Database=OkVolleyVibes;…`). `docker compose up -d`
starts SQL Server Express as an alternative — see `docker-compose.yml`. Migrations:

```bash
dotnet ef migrations add <Name> --project OkVolleyVibes.Infrastructure --startup-project OkVolleyVibes.Api --output-dir Persistence/Migrations
```

`AppDbContextFactory` lets the tools run without building the API host.
