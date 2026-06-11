# Task 03: Registration Endpoint

## Status

pending

## Wave

2

## Description

Wires the `RegisterUserRequest` to a `POST /api/auth/register` Minimal API endpoint. Adds the `AuthMapper` static class, the `AuthEndpoints` static class, and registers the Auth module in `ServiceCollectionExtensions`. Also adds BDD unit tests for the registration handler.

## Dependencies

**Depends on:** task-01-registration-handler.md, task-02-user-create-command.md
**Blocks:** STORY-006 (needs the Auth module registration pattern established here)

**Context from dependencies:**
- task-01 created `RegisterUserRequest`, `RegisterUserResponse`, `RegisterUserRequestHandler` in `Application/Auth/Features/RegisterUser/`
- task-02 created `CreateUserCommand` and `GetUserByEmailQuery` in `Data/Auth/`
- The `Result<T>` type is in `CM.TableNow.Shared`. `TypedResultHelper.ToResult<T>()` converts `Result<T>` to `IResult`
- STORY-001 `Program.cs` has a commented `// services.AddAuthModule(configuration);` stub ready to uncomment
- Mediator is source-generated — after adding new `IRequest`/`IRequestHandler` types, the project must build once to regenerate the source

## Files to Create

- `server/src/Api/Endpoints/AuthEndpoints.cs` — Minimal API route definitions for auth
- `server/src/Api/Mappers/AuthMapper.cs` — API contract ↔ Application model translation
- `server/src/Application/CM.TableNow.Auth.Application/Extensions/ServiceCollectionExtensions.cs` — `AddAuthModule()` extension
- `server/tests/UnitTests/Auth/describe_register_user/when_email_is_already_taken.cs`
- `server/tests/UnitTests/Auth/describe_register_user/when_request_is_valid.cs`

## Files to Modify

- `server/src/Api/Extensions/ServiceCollectionExtensions.cs` — Uncomment `services.AddAuthModule(configuration)`
- `server/src/Api/Program.cs` — Map auth endpoints

## Technical Details

### Code Snippets

```csharp
// Api/Mappers/AuthMapper.cs
using CM.TableNow.Auth.Application.Features.RegisterUser;
using CM.TableNow.Contracts;

namespace CM.TableNow.Api.Mappers;

public static class AuthMapper
{
    public static RegisterUserRequest ToRequest(RegisterUserApiRequest apiRequest) =>
        new(apiRequest.Name, apiRequest.Email, apiRequest.Password);
}
```

```csharp
// Api/Endpoints/AuthEndpoints.cs
using CM.TableNow.Api.Mappers;
using CM.TableNow.Auth.Application.Features.RegisterUser;
using CM.TableNow.Contracts;
using CM.TableNow.Shared;
using Mediator;

namespace CM.TableNow.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        var auth = group.MapGroup("/auth");

        auth.MapPost("/register", async (
            RegisterUserApiRequest apiRequest,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var request = AuthMapper.ToRequest(apiRequest);
            var result = await mediator.Send(request, ct);
            return TypedResultHelper.ToResult(result);
        })
        .WithName("RegisterUser")
        .Produces<RegisterUserResponse>(201)
        .ProducesProblem(400)
        .ProducesProblem(409);

        return group;
    }
}
```

Add to `Program.cs` after building the app:
```csharp
app.MapGroup("/api").MapAuthEndpoints();
```

BDD test example:
```csharp
// tests/UnitTests/Auth/describe_register_user/when_email_is_already_taken.cs
namespace describe_register_user;

public class when_email_is_already_taken : module_fixture
{
    [Fact]
    public async Task it_should_return_conflict_status()
    {
        // Arrange: mediator returns a non-null User for GetUserByEmailQuery
        Mediator.Send(Arg.Any<GetUserByEmailQuery>(), Arg.Any<CancellationToken>())
            .Returns(new User { Id = Guid.NewGuid(), Email = "existing@test.com" });

        var handler = new RegisterUserRequestHandler(Mediator);

        // Act
        var result = await handler.Handle(
            new RegisterUserRequest("Bob", "existing@test.com", "Password1!"),
            CancellationToken.None);

        // Assert
        result.StatusCode.Should().Be(409);
        result.IsSuccess.Should().BeFalse();
    }
}
```

### Contract Types (add to Contracts project)

```csharp
// server/src/Contracts/Auth/RegisterUserApiRequest.cs
namespace CM.TableNow.Contracts;

public record RegisterUserApiRequest(string Name, string Email, string Password);
```

## Acceptance Criteria

- [ ] `POST /api/auth/register` with valid body returns 201
- [ ] `POST /api/auth/register` with duplicate email returns 409
- [ ] `POST /api/auth/register` with missing fields returns 400
- [ ] BDD test namespace is `describe_register_user`; test classes are `when_email_is_already_taken` and `when_request_is_valid`
- [ ] `dotnet test` passes for the new tests
- [ ] `dotnet build` exits with code 0
