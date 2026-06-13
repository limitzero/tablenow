# Task 03: Login Application Layer

## Status

pending

## Wave

2

## Description

Implement the Application-layer `LoginRequestHandler` that orchestrates user authentication: fetches the user by email via `GetUserByEmailQuery`, runs BCrypt password comparison (always runs to prevent timing attacks), calls `JwtTokenGenerator` to produce the signed JWT, and returns `Result<LoginResponse>`. Invalid credentials (unknown email or wrong password) return a generic 401 without disclosing which field failed.

## Dependencies

**Depends on:** task-01-get-user-by-email-query.md, task-02-jwt-token-generator.md
**Blocks:** task-04-login-endpoint.md

**Context from dependencies:** task-01 defines `GetUserByEmailQuery(Email) → Result<UserData?>` where `UserData` contains `Id`, `Email`, `PasswordHash`, `Role`. Returning `Success(null)` means the email was not found. task-02 defines `JwtTokenGenerator.GenerateToken(Guid userId, string email, string role) → (string Token, DateTimeOffset ExpiresAt)` in `CM.TableNow.Infrastructure.Auth`.

## Files to Create

- `server/src/Application/Auth/Features/Login/LoginRequest.cs` — `LoginRequest` and `LoginResponse` records.
- `server/src/Application/Auth/Features/Login/LoginRequestHandler.cs` — Handler orchestrating lookup, BCrypt comparison, and token generation.

## Files to Modify

- None.

## Technical Details

### Implementation Steps

1. Define `LoginRequest(string Email, string Password)` implementing `IRequest<Result<LoginResponse>>`.
2. Define `LoginResponse(string Token, DateTimeOffset ExpiresAt)`.
3. Implement `LoginRequestHandler(IMediator mediator, JwtTokenGenerator tokenGenerator)` with primary constructor.
4. In the handler:
   a. Dispatch `GetUserByEmailQuery(request.Email)` via mediator.
   b. Store the `UserData?` result.
   c. Define a dummy hash constant for timing safety: `const string DummyHash = "$2a$12$..."` (a valid BCrypt hash).
   d. Run `BCrypt.Net.BCrypt.Verify(request.Password, userData?.PasswordHash ?? DummyHash)`.
   e. If `userData == null` or `!passwordValid`, return `Result<LoginResponse>.Failure(401, "Invalid credentials")`.
   f. Call `tokenGenerator.GenerateToken(userData.Id, userData.Email, userData.Role)`.
   g. Return `Result<LoginResponse>.Success(new LoginResponse(token, expiresAt))`.

### Code Snippets

```csharp
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Auth.Application.Features.Login;

public sealed record LoginRequest(string Email, string Password)
    : IRequest<Result<LoginResponse>>;

public sealed record LoginResponse(string Token, DateTimeOffset ExpiresAt);
```

```csharp
using BCrypt.Net;
using CM.TableNow.Auth.Data.Queries.GetUserByEmail;
using CM.TableNow.Infrastructure.Auth;
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Auth.Application.Features.Login;

public sealed class LoginRequestHandler(IMediator mediator, JwtTokenGenerator tokenGenerator)
    : IRequestHandler<LoginRequest, Result<LoginResponse>>
{
    // Pre-computed dummy hash to ensure constant-time comparison for unknown emails
    private const string DummyHash = "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewxXGq2Biu9M4tuu";

    public async ValueTask<Result<LoginResponse>> Handle(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var queryResult = await mediator.Send(new GetUserByEmailQuery(request.Email), cancellationToken);
        var userData = queryResult.Data;

        var hashToVerify = userData?.PasswordHash ?? DummyHash;
        var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, hashToVerify);

        if (userData is null || !passwordValid)
            return Result<LoginResponse>.Failure(401, "Invalid credentials");

        var (token, expiresAt) = tokenGenerator.GenerateToken(userData.Id, userData.Email, userData.Role);
        return Result<LoginResponse>.Success(new LoginResponse(token, expiresAt));
    }
}
```

## Acceptance Criteria

- [ ] Handler returns `Result<LoginResponse>` with a non-empty `Token` and future `ExpiresAt` for valid credentials.
- [ ] Handler returns 401 when the email is not found.
- [ ] Handler returns 401 when the password is wrong.
- [ ] BCrypt comparison runs even when the user is not found (prevents timing-based email enumeration).
- [ ] The 401 error message does not reveal which field (email or password) is wrong.

## Notes

- The `DummyHash` constant must be a valid BCrypt hash (not an empty string) to ensure `BCrypt.Verify` actually runs the full computation when the user doesn't exist.
- Refresh the `DummyHash` value periodically if it ever gets near its hardcoded BCrypt cost becoming too cheap relative to current hardware.
