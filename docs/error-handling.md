# Error handling

The API is **exception-based**. Application code throws; the API layer catches and renders a
response. Handlers and domain code never build HTTP results.

## Exception hierarchy (`OkVolleyVibes.Domain/Common/Exceptions/`)

```
Exception
└─ AppException (abstract)          ErrorCode + optional field Errors
   ├─ NotFoundException        → 404 Not Found
   ├─ ValidationException      → 400 Bad Request      (carries per-field Errors)
   ├─ ConflictException        → 409 Conflict
   ├─ BusinessRuleException    → 422 Unprocessable Entity
   └─ ForbiddenException       → 403 Forbidden
```

Anything that is **not** an `AppException` is an unexpected bug → `500` with a generic body.

### Using the categories

Throw a category directly for one-offs:

```csharp
throw new NotFoundException("Member", memberId);
throw new BusinessRuleException("Cannot check in outside your membership period.");
```

Derive a concrete type when the error is raised from several places or carries data — this is
the preferred form once a feature exists:

```csharp
public sealed class MemberNotFoundException(Guid id)
    : NotFoundException("member.not_found", $"Member '{id}' was not found.");
```

The central handler catches the **base** type, so derived exceptions map to the same status
automatically. Give each concrete type a stable `ErrorCode` (`area.reason`) — clients switch
on it for localized messages.

### 403 vs 404

For a resource the caller must not know exists, throw `NotFoundException` rather than
`ForbiddenException` so its existence isn't leaked. Use `ForbiddenException` when it's fine for
the caller to know the resource exists but not act on it.

## Response shape — RFC 9457 `ProblemDetails`

`Content-Type: application/problem+json`. Example (`400`):

```json
{
  "type": "https://httpstatuses.io/400",
  "title": "Bad Request",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "POST /api/payments",
  "errorCode": "validation.failed",
  "errors": { "amount": ["Amount must be greater than 0."] },
  "traceId": "00-<w3c-trace-id>-..."
}
```

| Field       | Always | Notes |
| ----------- | ------ | ----- |
| `type`      | yes    | Informational URL for the status code. |
| `title`     | yes    | Human-readable status name. |
| `status`    | yes    | HTTP status, mirrors the response code. |
| `detail`    | yes    | Safe-to-show message. Generic for `500`. |
| `instance`  | yes    | `"<METHOD> <path>"` of the request. |
| `errorCode` | yes    | Stable machine key. `server.unexpected` for `500`. |
| `errors`    | 400 only | `{ field: [messages] }`. |
| `traceId`   | yes    | W3C trace id — quote it in bug reports; it's in the server logs. |

Clients should branch on **`errorCode`** (and HTTP status), never on `detail` text.

## Where it lives

| Piece | Location |
| ----- | -------- |
| Exception types | `OkVolleyVibes.Domain/Common/Exceptions/` |
| `AppException` → status + ProblemDetails | `OkVolleyVibes.Api/ExceptionHandling/AppExceptionHandler.cs` |
| Catch-all → 500 | `OkVolleyVibes.Api/ExceptionHandling/GlobalExceptionHandler.cs` |
| `traceId` / `instance` injection | `AddProblemDetails(...)` in `OkVolleyVibes.Api/DependencyInjection.cs` |
| Manual test hook (Dev/Testing only) | `GET /_diag/throw/{kind}` — `Features/Diagnostics/ThrowEndpoint.cs` |

`kind` ∈ `notfound | validation | conflict | businessrule | forbidden | unexpected`.

## Logging

`AppException` → `Warning` (expected, no stack trace noise). Everything else → `Error` with the
full exception. The `traceId` ties a client-visible error to its log entry.

## Not yet wired

`ValidationException` is currently only thrown manually. Once the request dispatcher
(`ISender`) lands, a `ValidationBehavior` in the pipeline will collect FluentValidation
failures and throw it automatically — handlers will never see invalid input.
