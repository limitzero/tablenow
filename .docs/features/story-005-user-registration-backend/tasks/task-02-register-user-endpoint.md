# Task 02: Register User Endpoint & Tests

## Status

pending

## Wave

2

## Description

Registers the `POST /api/auth/register` Minimal API endpoint and writes BDD unit tests for the handler. The endpoint is the thin HTTP entry point — it sends a `RegisterUserRequest` via IMediator and maps the `Result<T>` to an HTTP response via `TypedResultHelper`.

## Dependencies

**Depends on:** task-01-register-user-handler.md
**Blocks:** STORY-006 (login depends on users being registerable), STORY-008 (frontend auth)

**Context from dependencies:** task-01 created `RegisterUserRequest`, `RegisterUserResponse`, and `RegisterUserRequestHandler`. The `TypedResultHelper` from STORY-001 task-02 maps Result status codes to IResult. This task only wires the HTTP layer.

## Files to Create

- `server/src/Api/Endpoints/AuthEndpoints.cs` — static class with `MapAuthEndpoints`
- `server/tests/UnitTests/Auth/describe_register_user/when_email_is_already_taken.cs`
- `server/tests/UnitTests/Auth/describe_register_user/when_request_is_valid.cs`

## Files to Modify

- `server/src/Api/Program.cs` — map auth endpoints group

## Technical Details

### Implementation Steps

1. Create `AuthEndpoints.cs` as a static class with a `MapAuthEndpoints(RouteGroupBuilder group)` method. Register `POST /register` with no auth required.

2. In `Program.cs`, map the group and call `MapAuthEndpoints`:
   ```csharp
   app.MapGroup("/api/auth").MapAuthEndpoints();
   ```

3. Write BDD unit tests using `xunit` and `Moq`. Use the `module_fixture` base class pattern: mock `IMediator`, set up the return value, call the handler directly, assert the result.

### Code Snippets

```csharp
// server/src/Api/Endpoints/AuthEndpoints.cs
namespace TableNow.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/register", async (
            RegisterUserRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(request, ct);
            return TypedResultHelper.ToResult(result);
        });

        // POST /login added in STORY-006
        return group;
    }
}
```

```csharp
// describe_register_user/when_email_is_already_taken.cs
namespace describe_register_user;

public class when_email_is_already_taken
{
    [Fact]
    public async Task it_should_return_conflict_status()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<RegisterUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RegisterUserResponse>.Failure("An account with this email already exists.", 409));

        var handler = new RegisterUserRequestHandler(mediator.Object);
        var result = await handler.Handle(
            new RegisterUserRequest("Alice", "alice@test.com", "P@ssw0rd123"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
    }
}
```

```csharp
// describe_register_user/when_request_is_valid.cs
namespace describe_register_user;

public class when_request_is_valid
{
    [Fact]
    public async Task it_should_return_created_status()
    {
        var userId = Guid.NewGuid();
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CreateUserResult>.Success(new CreateUserResult(userId, "Alice", "alice@test.com")));

        var handler = new RegisterUserRequestHandler(mediator.Object);
        var result = await handler.Handle(
            new RegisterUserRequest("Alice", "alice@test.com", "P@ssw0rd123"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data!.UserId.Should().Be(userId);
    }
}
```

## Acceptance Criteria

- [ ] `POST /api/auth/register` is mapped in `AuthEndpoints.MapAuthEndpoints`
- [ ] Endpoint sends `RegisterUserRequest` via IMediator and returns `TypedResultHelper.ToResult(result)`
- [ ] No auth required on the register endpoint (not in the protected group)
- [ ] `when_email_is_already_taken` test passes — asserts StatusCode 409
- [ ] `when_request_is_valid` test passes — asserts StatusCode 201 and userId present
