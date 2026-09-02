# Application — Common

Cross-cutting Application concerns:

- `Interfaces/` — ports the Application needs from the outside world
  (`IAppDbContext`, `IClock`, `IEmailSender`, …). Implemented in Infrastructure.
- `Behaviors/` — pipeline behaviors (validation, logging, exception → ProblemDetails,
  transaction-per-command).
- `Messaging/` — `ICommand` / `IQuery` / handler marker interfaces if not using a library.

Feature use cases live in `OkVolleyVibes.Application/<Feature>/<UseCase>/`
(Command/Query + Response + Handler + Validator). See the `dotnet-api` skill.
