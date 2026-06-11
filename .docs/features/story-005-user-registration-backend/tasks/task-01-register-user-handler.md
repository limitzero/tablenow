# Task 01: Register User Handler

## Status

pending

## Wave

1

## Description

Creates the complete Application + Data layer for user registration: the Application-layer request/handler (sends a Data command via IMediator), the Data-layer command/handler (EF insert with duplicate-email detection), and the FluentValidation validator. This is the full business logic — the endpoint in task-02 is just the HTTP entry point.

## Dependencies

**Depends on:** None (Wave 1 — but requires STORY-001 solution, STORY-003 domain entities, and STORY-004 seed data infrastructure)
**Blocks:** task-02-register-user-endpoint.md

**Context from dependencies:** STORY-001 created the Application/Auth and Data/Auth projects. STORY-003 created the `User` domain entity and `AppDbContext` with `DbSet<User>`. The `Result<T>` type from STORY-001 task-02 is available in Shared. This task adds the CQRS handlers on top of that foundation.

## Files to Create

- `server/src/Application/Auth/Features/RegisterUser/RegisterUserRequest.cs`
- `server/src/Application/Auth/Features/RegisterUser/RegisterUserResponse.cs`
- `server/src/Application/Auth/Features/RegisterUser/RegisterUserRequestHandler.cs`
- `server/src/Application/Auth/Features/RegisterUser/RegisterUserRequestValidator.cs`
- `server/src/Data/Auth/Commands/CreateUserCommand.cs`
- `server/src/Data/Auth/Commands/CreateUserCommandHandler.cs`

## Technical Details

### Implementation Steps

1. Create the Application-layer types in `Application/Auth/Features/RegisterUser/`.

2. Create the Data-layer command and handler in `Data/Auth/Commands/`.

3. The Application handler sends the Data command via `IMediator.Send()` — it does NOT access EF Core directly.

4. The Data handler checks for duplicate email, then inserts the user.

### Code Snippets

```csharp
// RegisterUserRequest.cs
namespace TableNow.Application.Auth.Features.RegisterUser;

public record RegisterUserRequest(string Name, string Email, string Password)
    : IRequest<Result<RegisterUserResponse>>;
```

```csharp
// RegisterUserResponse.cs
namespace TableNow.Application.Auth.Features.RegisterUser;

public record RegisterUserResponse(Guid UserId, string Name, string Email);
```

```csharp
// RegisterUserRequestHandler.cs
namespace TableNow.Application.Auth.Features.RegisterUser;

public class RegisterUserRequestHandler(IMediator mediator)
    : IRequestHandler<RegisterUserRequest, Result<RegisterUserResponse>>
{
    public async ValueTask<Result<RegisterUserResponse>> Handle(
        RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

        var command = new CreateUserCommand(request.Name, request.Email, passwordHash);
        var result = await mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Result<RegisterUserResponse>.Success(
                new RegisterUserResponse(result.Data!.UserId, result.Data.Name, result.Data.Email), 201)
            : Result<RegisterUserResponse>.Failure(result.Errors, result.StatusCode);
    }
}
```

```csharp
// RegisterUserRequestValidator.cs
namespace TableNow.Application.Auth.Features.RegisterUser;

public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
```

```csharp
// CreateUserCommand.cs
namespace TableNow.Data.Auth.Commands;

public record CreateUserCommand(string Name, string Email, string PasswordHash)
    : IRequest<Result<CreateUserResult>>;

public record CreateUserResult(Guid UserId, string Name, string Email);
```

```csharp
// CreateUserCommandHandler.cs
namespace TableNow.Data.Auth.Commands;

public class CreateUserCommandHandler(AppDbContext db)
    : IRequestHandler<CreateUserCommand, Result<CreateUserResult>>
{
    public async ValueTask<Result<CreateUserResult>> Handle(
        CreateUserCommand command, CancellationToken cancellationToken)
    {
        var exists = await db.Users.AnyAsync(u => u.Email == command.Email, cancellationToken);
        if (exists)
            return Result<CreateUserResult>.Failure("An account with this email already exists.", 409);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Email = command.Email,
            PasswordHash = command.PasswordHash,
            Role = "Diner",
            CreatedAt = DateTimeOffset.UtcNow,
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return Result<CreateUserResult>.Success(new CreateUserResult(user.Id, user.Name, user.Email));
    }
}
```

## Acceptance Criteria

- [ ] `RegisterUserRequest`, `RegisterUserResponse`, `RegisterUserRequestHandler`, and `RegisterUserRequestValidator` exist in `Application/Auth/Features/RegisterUser/`
- [ ] `CreateUserCommand` and `CreateUserCommandHandler` exist in `Data/Auth/Commands/`
- [ ] Handler returns `Result` with StatusCode 201 on success
- [ ] Handler returns `Result` with StatusCode 409 when email already exists
- [ ] Validator rejects missing name, invalid email, password shorter than 8 chars
- [ ] Password hashing uses `BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12)`
