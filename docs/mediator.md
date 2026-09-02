# Mediator (`OkVolleyVibes.Mediator`)

A ~120-line in-house mediator — no MediatR. Just what the use-case pipeline needs: send a
request, run it through ordered pipeline behaviors, reach exactly one handler.

Depends only on `Microsoft.Extensions.DependencyInjection.Abstractions`. `Domain` must never
reference it (enforced by an architecture test); `Application` and `Api` do.

## Contract

```csharp
public interface IRequest<out TResponse>;                       // marker; Unit for "no result"

public interface IRequestHandler<in TRequest, TResponse>        // exactly one per request type
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

public interface IPipelineBehavior<in TRequest, TResponse>      // cross-cutting wrapper
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct);
}

public interface ISender
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);
}
```

## Registration

```csharp
services.AddMediator(ApplicationAssemblyReference.Assembly);   // scans for IRequestHandler<,>, registers ISender

// behaviors are registered by the caller, as open generics, in execution order (first = outermost):
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

`AddMediator` only wires handlers + `ISender`. Behaviors stay explicit so their order is obvious
at the call site. `ISender` and handlers are **scoped**.

## Execution order

For the registration above, `sender.Send(new FooCommand())` runs:

```
LoggingBehavior         → ValidationBehavior        → FooHandler
  (log "Handling")           (run validators)
                             throws ValidationException on failure — handler never called
  (log "Handled"/"failed")
```

`ISender` resolves a cached wrapper per request type (`ConcurrentDictionary<Type, …>`), pulls the
handler and behaviors from the current scope, folds the behaviors around the handler call
inner-to-outer, and invokes the chain. Unknown request type → `InvalidOperationException`
("No handler registered for …").

## Pipeline behaviors (`OkVolleyVibes.Application/Common/Behaviors/`)

| Behavior | Does |
| -------- | ---- |
| `LoggingBehavior<,>` | logs start / completion / failure of every request |
| `ValidationBehavior<,>` | runs all `IValidator<TRequest>` (FluentValidation); on failure throws the domain `ValidationException` → 400 ProblemDetails (see `error-handling.md`). Handlers only ever see valid input. |

## Adding a use case

```
Application/<Feature>/<UseCase>/
  <UseCase>Command.cs     // : IRequest<<UseCase>Response>
  <UseCase>Response.cs
  <UseCase>Handler.cs     // : IRequestHandler<…>   (internal sealed)
  <UseCase>Validator.cs   // : AbstractValidator<<UseCase>Command>   (internal sealed)
```

The endpoint (in `Api/Features/<Feature>/`) injects `ISender` and calls `Send`. Reference
example: `Application/Diagnostics/Ping/*` + `Api/Features/Diagnostics/PingEndpoint.cs`
(`GET /_diag/ping?message=…`, Dev/Testing only).
