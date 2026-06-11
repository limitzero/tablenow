# Task 01: Registration Application Handler

## Status

pending

## Wave

1

## Description

Implements the `RegisterUserRequest` / `RegisterUserResponse` / `RegisterUserRequestHandler` in the Application/Auth layer. The handler validates the request via FluentValidation, checks for duplicate email by dispatching a data query, hashes the password via BCrypt, then dispatches a `CreateUserCommand`. Returns `Result<RegisterUserResponse>`.

## Dependencies

**Depends on:** STORY-001 task-01-solution-projects.md, STORY-003 task-01-domain-entities.md
**Blocks:** task-03-registration-endpoint.md

**Context from dependencies:** 
- STORY-001 set up `CM.TableNow.Auth.Application.csproj` with Mediator and FluentValidation packages
- STORY-003 created the `User` domain entity: `Id (Guid)`, `Name`, `Email`, `PasswordHash`, `Role`, `CreatedAt`
- The `Result<T>` type from Shared has `Data`, `Errors`, `StatusCode`, `IsSuccess` and factory methods: `Result.Created(data)`, `Result.Conflict(message)`, `Result.BadRequest(message)`

## Files to Create

- `server/src/Application/CM.TableNow.Auth.Application/Features/RegisterUser/RegisterUserRequest.cs`
- `server/src/Application/CM.TableNow.Auth.Application/Features/RegisterUser/RegisterUserResponse.cs`
- `server/src/Application/CM.TableNow.Auth.Application/Features/RegisterUser/RegisterUserRequestHandler.cs`
- `server/src/Application/CM.TableNow.Auth.Application/Features/RegisterUser/RegisterUserValidator.cs`

## Files to Modify

None.

## Technical Details

### Code Snippets

```csharp
// RegisterUserRequest.cs
namespace CM.TableNow.Auth.Application.Features.RegisterUser;

public record RegisterUserRequest(string Name, string Email, string Password);
```

```csharp
// RegisterUserResponse.cs
namespace CM.TableNow.Auth.Application.Features.RegisterUser;

public record RegisterUserResponse(Guid UserId, string Name, string Email);
```

```csharp
// RegisterUserValidator.cs
using FluentValidation;

namespace CM.TableNow.Auth.Application.Features.RegisterUser;

public class RegisterUserValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
```

```csharp
// RegisterUserRequestHandler.cs
using CM.TableNow.Auth.Data.Queries.GetUserByEmail;
using CM.TableNow.Auth.Data.Commands.CreateUser;
using CM.TableNow.Shared;
using Mediator;

namespace CM.TableNow.Auth.Application.Features.RegisterUser;

public class RegisterUserRequestHandler(IMediator mediator) : IRequestHandler<RegisterUserRequest, Result<RegisterUserResponse>>
{
    public async ValueTask<Result<RegisterUserResponse>> Handle(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        // Validate
        var validator = new RegisterUserValidator();
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return ResultExtensions.BadRequest<RegisterUserResponse>(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        // Check duplicate email
        var existingUser = await mediator.Send(
            new GetUserByEmailQuery(request.Email), cancellationToken);
        if (existingUser is not null)
            return ResultExtensions.Conflict<RegisterUserResponse>(
                $"Email '{request.Email}' is already registered.");

        // Hash password
        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

        // Create user
        var userId = await mediator.Send(
            new CreateUserCommand(request.Name, request.Email, hash), cancellationToken);

        return ResultExtensions.Created(new RegisterUserResponse(userId, request.Name, request.Email));
    }
}
```

### API Endpoint (for reference — implemented in task-03)

```
POST /api/auth/register
Body: { "name": "string", "email": "string", "password": "string" }
201: { "userId": "guid", "name": "string", "email": "string" }
400: { errors: [...] }
409: { errors: [...] }
```

## Acceptance Criteria

- [ ] `RegisterUserRequest`, `RegisterUserResponse`, `RegisterUserRequestHandler`, `RegisterUserValidator` exist at specified paths
- [ ] Handler returns `Result.Created(...)` on success with status code 201
- [ ] Handler returns `Result.Conflict(...)` when email already exists
- [ ] Handler returns `Result.BadRequest(...)` when validation fails
- [ ] `dotnet build` exits with code 0

## Notes

`GetUserByEmailQuery` and `CreateUserCommand` are created in task-02 (running in parallel). The handler file references those types — ensure both tasks produce compatible signatures before building.
