# Task 02: Login Application Handler

## Status

pending

## Wave

2

## Description

Implements `LoginRequest` / `LoginResponse` / `LoginRequestHandler` in the Application/Auth layer. The handler looks up the user by email, verifies the BCrypt password hash, then calls `JwtTokenGenerator` to produce the token. Returns `Result<LoginResponse>` with the token on success, or 401 on invalid credentials.

## Dependencies

**Depends on:** task-01-jwt-infrastructure.md
**Blocks:** task-03-login-endpoint.md

**Context from dependencies:**
- task-01 created `JwtTokenGenerator` with method `GenerateToken(Guid userId, string email, string role)` returning `(string Token, DateTime ExpiresAt)`
- `JwtTokenGenerator` is registered as a singleton via `AddAuthInfrastructure()`
- `GetUserByEmailQuery` was created in STORY-005 task-02 in `Data/Auth/Queries/GetUserByEmail/`
- `User` entity: `Id (Guid)`, `Email`, `PasswordHash`, `Role`

## Files to Create

- `server/src/Application/CM.TableNow.Auth.Application/Features/Login/LoginRequest.cs`
- `server/src/Application/CM.TableNow.Auth.Application/Features/Login/LoginResponse.cs`
- `server/src/Application/CM.TableNow.Auth.Application/Features/Login/LoginRequestHandler.cs`

## Files to Modify

None.

## Technical Details

### Code Snippets

```csharp
// LoginRequest.cs
namespace CM.TableNow.Auth.Application.Features.Login;

public record LoginRequest(string Email, string Password);
```

```csharp
// LoginResponse.cs
namespace CM.TableNow.Auth.Application.Features.Login;

public record LoginResponse(string Token, DateTime ExpiresAt);
```

```csharp
// LoginRequestHandler.cs
using CM.TableNow.Auth.Data.Queries.GetUserByEmail;
using CM.TableNow.Auth.Infrastructure;
using CM.TableNow.Shared;
using Mediator;

namespace CM.TableNow.Auth.Application.Features.Login;

public class LoginRequestHandler(IMediator mediator, JwtTokenGenerator tokenGenerator)
    : IRequestHandler<LoginRequest, Result<LoginResponse>>
{
    public async ValueTask<Result<LoginResponse>> Handle(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await mediator.Send(new GetUserByEmailQuery(request.Email), cancellationToken);

        // Return the same 401 for both "not found" and "wrong password" — no field-level detail
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return ResultExtensions.Unauthorized<LoginResponse>();

        var (token, expiresAt) = tokenGenerator.GenerateToken(user.Id, user.Email, user.Role);
        return ResultExtensions.Ok(new LoginResponse(token, expiresAt));
    }
}
```

## Acceptance Criteria

- [ ] `LoginRequest`, `LoginResponse`, `LoginRequestHandler` exist at specified paths
- [ ] Valid credentials return `Result.Ok(...)` with token and expiry
- [ ] Invalid email OR wrong password both return 401 — same response, no field-level detail
- [ ] `dotnet build` exits with code 0
