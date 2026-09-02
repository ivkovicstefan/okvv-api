# Api — Features

One folder per feature; one **REPR endpoint per file**, e.g.
`Features/Payments/CreatePaymentEndpoint.cs`.

Each endpoint:

- is a `sealed class` implementing `IEndpoint`;
- maps a single route in a `MapGroup("/api/<feature>")` with `.WithTags(...)` and `.RequireAuthorization(...)`;
- binds a request type, calls the Application handler via `ISender`, maps the response to `Results.*`;
- contains **no business logic and no EF Core** — transport only;
- declares `.Produces<T>()` / `.ProducesValidationProblem()` so the OpenAPI document stays accurate.

See the `dotnet-api` skill for the full contract. `Health/` is the reference example.
