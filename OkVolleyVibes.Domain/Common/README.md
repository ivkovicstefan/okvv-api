# Domain — Common

Building blocks shared across the domain: base entity / aggregate root, value objects,
domain events, strongly-typed IDs.

- `Exceptions/` — `AppException` and its categories (`NotFoundException`, `ValidationException`,
  `ConflictException`, `BusinessRuleException`, `ForbiddenException`). See `docs/error-handling.md`.

The Domain layer has **zero dependencies** on other projects or infrastructure.
See the `dotnet-api` skill.
