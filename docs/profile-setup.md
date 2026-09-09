# Initial profile setup (FR-A11)

Onboarding flow: **register → verify email (signs in) → complete the survey**. The survey is
**mandatory** — until it's submitted, any endpoint behind the `ProfileComplete` policy returns
`403 { "errorCode": "profile.incomplete" }`.

## Sessions

- Verifying the email (`GET /api/auth/verify-email`) confirms the address **and** returns
  `{ accessToken, refreshToken, accessTokenExpiresAtUtc }`.
- Access token: JWT, 15 min, HMAC-SHA256. Claims: `sub`, `email`, `role` (repeated), `profile_completed`.
- Refresh token: 30 days, opaque, stored **only as a SHA-256 hash** (`RefreshTokens`). Rotated on
  every `POST /api/auth/refresh`; reusing a rotated/revoked token → `400`. `POST /api/auth/logout`
  revokes one.
- `Jwt` config in `appsettings*.json`; `Jwt:SigningKey` (≥ 32 bytes) is required — startup fails without it.

## The onboarding gate

- `AddAuthorizationBuilder`: default policy = authenticated; policy **`ProfileComplete`** =
  authenticated **+ `profile_completed=true`** claim.
- `ProblemDetailsAuthorizationResultHandler` turns failures into ProblemDetails:
  `auth.unauthorized` (401), `profile.incomplete` (403), `access.forbidden` (403).
- Exempt (auth-only, so an onboarding user can reach them): `POST /api/account/complete-profile`,
  `GET /api/account/profile`, `POST /api/account/photo`. Anonymous: `/api/auth/*`, `/health`, swagger.
- Future feature endpoints opt in with `.RequireAuthorization("ProfileComplete")`.

## `POST /api/account/complete-profile`

Authenticated. Body:

| Field | Rule |
| --- | --- |
| `dateOfBirth` (`yyyy-MM-dd`) | age 5–100 |
| `preferredLanguage` | `en` / `sr-Latn` / `ru` |
| `skillRating` | 1–10 |
| `hasTrainedBefore` | bool |
| `trainingHistory` | required + ≤ 2000 chars **when** `hasTrainedBefore` (clubs & periods) |
| `positions` | ≥ 1 of `Setter` / `OutsideHitter` / `Opposite` / `MiddleBlocker` / `Libero` **when** `hasTrainedBefore` |
| `recreationalExperience` | `Never` / `ALittleBit` / `AFewTimes` / `ALotOfTimes` — required **when NOT** `hasTrainedBefore` |
| `clubInterest` | `Recreational` / `Competitive` / `Both` |
| `agreedToClubRules` | **must be `true`** |

Enums are serialized as strings (`JsonStringEnumConverter`). On success: writes `User.DateOfBirth` +
`PreferredLanguage`, creates the `OnboardingSurvey`, stamps `User.ProfileCompletedAtUtc`, and returns a
**new token pair** with `profile_completed=true` (client should replace its tokens). Submitting twice
→ `409`. The whole thing runs in one transaction (`ITransactionalRequest`).

## `GET /api/account/profile`

Returns identity + basic profile + `roles` + `profileCompleted` + `hasPhoto` + a `survey` summary
(null until completed).

## Profile photo

- `POST /api/account/photo` — `multipart/form-data`, field `file`. JPEG / PNG / WebP, ≤ **512 KB**,
  magic-byte sniffed. Stored as bytes in `UserPhotos` (one per user) — move to object storage later.
- `GET /api/account/photo/{userId}` — the image (any signed-in user may view any member's photo);
  `404` if none.

## Data model added this feature

`User.DateOfBirth`, `User.ProfileCompletedAtUtc`; tables `OnboardingSurveys`, `RefreshTokens`,
`UserPhotos` (migration `ProfileSetupAndTokens`). The three seeded founder accounts are left
onboarding-incomplete on purpose.
