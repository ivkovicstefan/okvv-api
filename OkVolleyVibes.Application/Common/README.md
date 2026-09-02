# Application — Common

Cross-cutting Application concerns:

- `Interfaces/` — ports the Application needs from the outside world
  (`IAppDbContext`, `IClock`, `IEmailSender`, …). Implemented in Infrastructure.
- `Behaviors/` — mediator pipeline behaviors. `LoggingBehavior<,>` and `ValidationBehavior<,>`
  exist; add transaction-per-command here later. See `docs/mediator.md`.

Messaging contracts (`IRequest`, `IRequestHandler`, `IPipelineBehavior`, `ISender`) come from the
`OkVolleyVibes.Mediator` project.

Feature use cases live in `OkVolleyVibes.Application/<Feature>/<UseCase>/`
(Command/Query + Response + Handler + Validator). See the `dotnet-api` skill and `docs/mediator.md`.
