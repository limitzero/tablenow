# Task 02: RegisterUser Application Layer

## Status

pending

## Wave

1

## Description

Implement the Application-layer handler for user registration. `RegisterUserRequestHandler` validates the incoming request (non-empty name, valid email format, password ≥ 8 characters), hashes the password with BCrypt, dispatches `CreateUserCommand` to the Data layer, and maps the result to `RegisterUserResponse`. On validation failure it returns a `Result` with status 400; on duplicate email it propagates the 409 from the Data layer.

## Dependencies

**Depends on:** None (Wave 1) — contracts from task-01 (`CreateUserCommand`) are stable enough to code against; both tasks can run concurrently.
**Blocks:** task-03-register-endpoint.md

**Context from dependencies:** task-01 defines `CreateUserCommand(Name, Email, PasswordHash) : ICommand<Result<Guid>>` in `CM.TableNow.Auth.Data`. This handler dispatches that command via `IMediator` — it does NOT reference the Data handler directly. `Result<T>` is in `CM.TableNow.Shared`.

## Files to Create

- `server/src/Application/Auth/Features/RegisterUser/RegisterUserRequest.cs` — `RegisterUserRequest` and `RegisterUserResponse` records.
- `server/src/Application/Auth/Features/RegisterUser/RegisterUserRequestHandler.cs` — Handler with validation, BCrypt hashing, and command dispatch.

## Files to Modify

- None (additive to Application project).

## Technical Details

### Implementation Steps

1. Add `BCrypt.Net-Next` to the Application/Auth project: `dotnet add server/src/Application/Auth package BCrypt.Net-Next`.
2. Define `RegisterUserRequest(string Name, string Email, string Password)` implementing `IRequest<Result<RegisterUserResponse>>`.
3. Define `RegisterUserResponse(Guid UserId, string Name, string Email)`.
4. Implement `RegisterUserRequestHandler(IMediator mediator)` with primary constructor.
5. In the handler, validate:
   - `string.IsNullOrWhiteSpace(request.Name)` → `Result.Failure(400, "Name is required")`
   - `!IsValidEmail(request.Email)` → `Result.Failure(400, "Invalid email format")`
   - `request.Password.Length < 8` → `Result.Failure(400, "Password must be at least 8 characters")`
6. Hash: `var hash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12)`.
7. Dispatch: `var result = await mediator.Send(new CreateUserCommand(request.Name, request.Email, hash), ct)`.
8. On failure propagate `result`'s status; on success map to `RegisterUserResponse`.

### Code Snippets

```csharp
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Auth.Application.Features.RegisterUser;

public sealed record RegisterUserRequest(string Name, string Email, string Password)
    : IRequest<Result<RegisterUserResponse>>;

public sealed record RegisterUserResponse(Guid UserId, string Name, string Email);
```

```csharp
using BCrypt.Net;
using CM.TableNow.Auth.Data.Commands.CreateUser;
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Auth.Application.Features.RegisterUser;

public sealed class RegisterUserRequestHandler(IMediator mediator)
    : IRequestHandler<RegisterUserRequest, Result<RegisterUserResponse>>
{
    public async ValueTask<Result<RegisterUserResponse>> Handle(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<RegisterUserResponse>.Failure(400, "Name is required");

        if (!request.Email.Contains('@'))
            return Result<RegisterUserResponse>.Failure(400, "Invalid email format");

        if (request.Password.Length < 8)
            return Result<RegisterUserResponse>.Failure(400, "Password must be at least 8 characters");

        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);
        var command = new CreateUserCommand(request.Name, request.Email, hash);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return Result<RegisterUserResponse>.Failure(result.StatusCode, result.Errors);

        return Result<RegisterUserResponse>.Success(
            new RegisterUserResponse(result.Data, request.Name, request.Email));
    }
}
```

## Acceptance Criteria

- [ ] Handler returns 400 when `Name` is empty.
- [ ] Handler returns 400 when `Email` does not contain `@`.
- [ ] Handler returns 400 when `Password` is fewer than 8 characters.
- [ ] Handler dispatches `CreateUserCommand` with the BCrypt hash (not the plaintext password).
- [ ] Handler returns 409 when the Data layer reports a duplicate email.
- [ ] Handler returns 201-compatible `Result<RegisterUserResponse>` with correct `UserId`, `Name`, `Email` on success.

## Notes

- Work factor 12 adds ~200–400ms of BCrypt computation per registration — this is intentional and acceptable for a security-sensitive operation.
- The email validation here is minimal (contains `@`). More thorough validation can be added later without breaking the interface.
