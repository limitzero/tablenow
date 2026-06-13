# Task 03: Register Endpoint & BDD Tests

## Status

pending

## Wave

2

## Description

Add the `POST /api/auth/register` Minimal API endpoint, the `AuthMapper` static class for API model ↔ Application model translation, and BDD unit tests covering the valid registration path and the duplicate-email conflict path. This is the public-facing entry point for user registration; it delegates all logic to the Application layer via `IMediator`.

## Dependencies

**Depends on:** task-01-create-user-command.md, task-02-register-user-application.md
**Blocks:** None

**Context from dependencies:** task-01 provides `CreateUserCommand` in `CM.TableNow.Auth.Data.Commands.CreateUser`. task-02 provides `RegisterUserRequest(Name, Email, Password)` → `Result<RegisterUserResponse(UserId, Name, Email)>` in `CM.TableNow.Auth.Application.Features.RegisterUser`. The endpoint maps the HTTP request body to `RegisterUserRequest` and the result to an HTTP response using `TypedResultHelper`.

## Files to Create

- `server/src/Api/Endpoints/Auth/AuthEndpoints.cs` — `static class AuthEndpoints` with `MapAuthEndpoints(RouteGroupBuilder)`.
- `server/src/Api/Endpoints/Auth/AuthMapper.cs` — Static mapper: `RegisterRequest` (API DTO) → `RegisterUserRequest` (Application).
- `server/src/Contracts/Auth/RegisterRequest.cs` — API-facing request DTO.
- `server/tests/UnitTests/Auth/describe_register_user.cs` — BDD test class.

## Files to Modify

- `server/src/Api/Program.cs` — Call `MapAuthEndpoints` on the `/api/auth` route group.

## Technical Details

### Implementation Steps

1. Define `RegisterRequest` API DTO with `Name`, `Email`, `Password` string properties in `Contracts/Auth/`.
2. Create `AuthMapper` with a static method `ToRequest(RegisterRequest r) => new RegisterUserRequest(r.Name, r.Email, r.Password)`.
3. In `AuthEndpoints.MapAuthEndpoints`, register:
   ```csharp
   group.MapPost("/register", async (RegisterRequest body, IMediator mediator, CancellationToken ct) =>
   {
       var result = await mediator.Send(AuthMapper.ToRequest(body), ct);
       return TypedResultHelper.ToResult(result);
   });
   ```
4. The auth group should NOT use `.RequireAuthorization()` — these endpoints are public.
5. Write BDD tests in `describe_register_user`:
   - `when_email_is_already_taken` → mock handler returns `Result.Failure(409, ...)` → assert handler returns 409 result.
   - `when_request_is_valid` → mock handler returns `Result.Success(new RegisterUserResponse(...))` → assert handler returns 201-equivalent.

### Code Snippets

```csharp
// Tests namespace pattern
namespace describe_register_user;

public class when_email_is_already_taken : module_fixture
{
    [Fact]
    public async Task it_should_return_conflict_status()
    {
        Mediator.Setup(m => m.Send(It.IsAny<RegisterUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RegisterUserResponse>.Failure(409, "Email already registered"));

        var handler = new RegisterUserRequestHandler(Mediator.Object);
        var result = await handler.Handle(new RegisterUserRequest("Alice", "alice@test.com", "Password123!"), default);

        result.StatusCode.Should().Be(409);
        result.IsSuccess.Should().BeFalse();
    }
}
```

## Acceptance Criteria

- [ ] `POST /api/auth/register` with valid body returns 201 and a JSON body with `userId`, `name`, `email`.
- [ ] `POST /api/auth/register` with a duplicate email returns 409 Conflict.
- [ ] `POST /api/auth/register` with missing fields returns 400 Bad Request.
- [ ] `AuthMapper.ToRequest` correctly maps `RegisterRequest` → `RegisterUserRequest`.
- [ ] BDD tests `when_email_is_already_taken` and `when_request_is_valid` pass.

## Notes

- `TypedResultHelper.ToResult(result)` maps status 201 → `Results.Created(...)`, 400 → `Results.BadRequest(...)`, 409 → `Results.Conflict(...)`.
- The `/api/auth` group is public — do not add `.RequireAuthorization()` to it.
