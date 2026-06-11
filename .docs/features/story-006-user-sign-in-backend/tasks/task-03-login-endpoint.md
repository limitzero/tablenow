# Task 03: Login Endpoint

## Status

pending

## Wave

3

## Description

Adds `POST /api/auth/login` to `AuthEndpoints.cs` and writes a BDD unit test verifying the 401 path. The endpoint is the thin HTTP wrapper — it sends `LoginRequest` via IMediator and maps the result.

## Dependencies

**Depends on:** task-02-login-handler.md
**Blocks:** STORY-007 (JWT middleware needs a login endpoint to test against), STORY-008 (frontend needs this endpoint)

**Context from dependencies:** task-02 created `LoginRequest`, `LoginResponse`, and `LoginRequestHandler`. STORY-005 task-02 created `AuthEndpoints.cs` with the `/register` endpoint. This task adds `/login` to the same static class and writes one BDD test.

## Files to Modify

- `server/src/Api/Endpoints/AuthEndpoints.cs` — add POST /login route

## Files to Create

- `server/tests/UnitTests/Auth/describe_user_login/when_credentials_are_invalid.cs`

## Technical Details

### Code Snippets

```csharp
// Add to AuthEndpoints.MapAuthEndpoints():
group.MapPost("/login", async (
    LoginRequest request,
    IMediator mediator,
    CancellationToken ct) =>
{
    var result = await mediator.Send(request, ct);
    return TypedResultHelper.ToResult(result);
});
```

```csharp
// describe_user_login/when_credentials_are_invalid.cs
namespace describe_user_login;

public class when_credentials_are_invalid
{
    [Fact]
    public async Task it_should_return_unauthorized_status()
    {
        var mediator = new Mock<IMediator>();
        var tokenGen = new Mock<JwtTokenGenerator>(Mock.Of<IOptions<JwtOptions>>());

        mediator.Setup(m => m.Send(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Failure("User not found.", 404));

        var handler = new LoginRequestHandler(mediator.Object, tokenGen.Object);
        var result = await handler.Handle(
            new LoginRequest("bad@example.com", "wrongpassword"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401);
        result.Errors.Should().Contain("Invalid credentials.");
    }
}
```

## Acceptance Criteria

- [ ] `POST /api/auth/login` is mapped in `AuthEndpoints.cs`
- [ ] Endpoint sends `LoginRequest` via IMediator and returns `TypedResultHelper.ToResult(result)`
- [ ] BDD test `it_should_return_unauthorized_status` passes
- [ ] No auth required on the login endpoint
