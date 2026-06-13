# Task 04: Login Endpoint & BDD Tests

## Status

pending

## Wave

3

## Description

Add the `POST /api/auth/login` Minimal API endpoint to `AuthEndpoints`, the `LoginRequest` API DTO and its mapper entry, and BDD unit tests covering valid credentials and invalid credentials cases. The endpoint is public (no `.RequireAuthorization()`). On success it returns 200 with `{ token, expiresAt }`; on failure it returns 401.

## Dependencies

**Depends on:** task-03-login-application.md
**Blocks:** None

**Context from dependencies:** task-03 defines `LoginRequest(Email, Password) → Result<LoginResponse(Token, ExpiresAt)>` in `CM.TableNow.Auth.Application.Features.Login`. task-01 and task-02 (from STORY-005) already created `AuthEndpoints.cs` and `AuthMapper.cs` — this task extends them with the login route.

## Files to Create

- `server/src/Contracts/Auth/LoginRequest.cs` — API-facing login DTO with `Email` and `Password`.
- `server/tests/UnitTests/Auth/describe_login.cs` — BDD test class for login handler.

## Files to Modify

- `server/src/Api/Endpoints/Auth/AuthEndpoints.cs` — Add `POST /login` route.
- `server/src/Api/Endpoints/Auth/AuthMapper.cs` — Add `ToLoginRequest(LoginRequest r)` mapper method.

## Technical Details

### Implementation Steps

1. Define `LoginRequest` API DTO with `Email` and `Password` strings in `Contracts/Auth/`.
2. Add `ToLoginRequest(LoginRequest r) => new Application.LoginRequest(r.Email, r.Password)` to `AuthMapper`.
3. In `AuthEndpoints.MapAuthEndpoints`, add:
   ```csharp
   group.MapPost("/login", async (LoginRequest body, IMediator mediator, CancellationToken ct) =>
   {
       var result = await mediator.Send(AuthMapper.ToLoginRequest(body), ct);
       return TypedResultHelper.ToResult(result);
   });
   ```
4. Write BDD tests:
   - `when_credentials_are_valid` → mock handler returns `Result.Success(new LoginResponse("token123", futureDate))` → assert status 200 and token present.
   - `when_credentials_are_invalid` → mock handler returns `Result.Failure(401, "Invalid credentials")` → assert status 401.

### Code Snippets

```csharp
namespace describe_login;

public class when_credentials_are_valid : module_fixture
{
    [Fact]
    public async Task it_should_return_token()
    {
        var expected = new LoginResponse("jwt-token", DateTimeOffset.UtcNow.AddHours(24));
        Mediator.Setup(m => m.Send(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LoginResponse>.Success(expected));

        var handler = new LoginRequestHandler(Mediator.Object, /* tokenGenerator mock */);
        var result = await handler.Handle(new LoginRequest("alice@test.com", "Password123!"), default);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Token.Should().NotBeEmpty();
    }
}

public class when_credentials_are_invalid : module_fixture
{
    [Fact]
    public async Task it_should_return_unauthorized()
    {
        Mediator.Setup(m => m.Send(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserData?>.Success(null));

        var handler = new LoginRequestHandler(Mediator.Object, TokenGenerator);
        var result = await handler.Handle(new LoginRequest("nobody@test.com", "wrong"), default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401);
    }
}
```

## Acceptance Criteria

- [ ] `POST /api/auth/login` with valid credentials returns 200 with `token` and `expiresAt`.
- [ ] `POST /api/auth/login` with wrong credentials returns 401 without detail about which field is wrong.
- [ ] BDD tests `when_credentials_are_valid` and `when_credentials_are_invalid` pass.
- [ ] `AuthMapper.ToLoginRequest` correctly maps the API DTO to the Application request.

## Notes

- The `/api/auth/login` endpoint is public — do not add `.RequireAuthorization()`.
- `TypedResultHelper.ToResult` for a 200 success should use `Results.Ok(result.Data)`.
