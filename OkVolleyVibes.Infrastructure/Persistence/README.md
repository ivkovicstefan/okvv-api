# Infrastructure — Persistence

EF Core 10 against MSSQL:

- `AppDbContext : DbContext, IAppDbContext`
- `Configurations/` — one `IEntityTypeConfiguration<T>` per aggregate
- `Migrations/` — generated migrations (one per logical change)
- Interceptors (auditing, domain-event dispatch), value converters

See the `dotnet-api` skill for query, migration, and MSSQL conventions.
