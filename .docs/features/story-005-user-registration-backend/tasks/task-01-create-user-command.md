# Task 01: CreateUser Data Command

## Status

pending

## Wave

1

## Description

Implement the Data-layer CQRS command that persists a new user in the `AuthDbContext`. `CreateUserCommand` accepts the already-validated and BCrypt-hashed fields from the Application layer. The handler checks for a duplicate email before inserting; on conflict it returns a `Result` failure with status 409. On success it returns the new user's `Guid` as `Result<Guid>`. This is the only write path to the `Users` table for registration.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-03-register-endpoint.md

**Context from dependencies:** Assumes STORY-001 created the `CM.TableNow.Auth.Data` project with the Mediator source generator configured. Assumes STORY-003 created the `User` EF model (`Id`, `Name`, `Email`, `PasswordHash`, `Role`, `CreatedAt`) and the `AuthDbContext` with a `Users` `DbSet<User>`.

## Files to Create

- `server/src/Data/Auth/Commands/CreateUser/CreateUserCommand.cs` — `CreateUserCommand` record with `ICommand<Result<Guid>>`.
- `server/src/Data/Auth/Commands/CreateUser/CreateUserCommandHandler.cs` — Handler that checks duplicate email and inserts the user.

## Files to Modify

- None (additive to existing Data project).

## Technical Details

### Implementation Steps

1. Define `CreateUserCommand` as a `record` with `Name`, `Email`, `PasswordHash`, implementing `ICommand<Result<Guid>>` from Mediator.
2. Implement `CreateUserCommandHandler` with primary constructor taking `AuthDbContext db`.
3. In the handler, check `await db.Users.AnyAsync(u => u.Email == command.Email, cancellationToken)`. If true, return `Result<Guid>.Failure(409, "Email already registered")`.
4. Create the `User` entity with `Id = Guid.NewGuid()`, `Role = "Diner"`, `CreatedAt = DateTimeOffset.UtcNow`.
5. `db.Users.Add(user)`, `await db.SaveChangesAsync(cancellationToken)`.
6. Return `Result<Guid>.Success(user.Id)`.

### Code Snippets

```csharp
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Auth.Data.Commands.CreateUser;

public sealed record CreateUserCommand(
    string Name,
    string Email,
    string PasswordHash) : ICommand<Result<Guid>>;
```

```csharp
using CM.TableNow.Auth.Domain.Entities;
using CM.TableNow.Shared.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Auth.Data.Commands.CreateUser;

public sealed class CreateUserCommandHandler(AuthDbContext db)
    : ICommandHandler<CreateUserCommand, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(u => u.Email == command.Email, cancellationToken))
            return Result<Guid>.Failure(409, "Email already registered");

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

        return Result<Guid>.Success(user.Id);
    }
}
```

## Acceptance Criteria

- [ ] `CreateUserCommandHandler` returns `Result<Guid>.Success(userId)` when the email is unique.
- [ ] `CreateUserCommandHandler` returns a failure result with status code 409 when the email is already taken.
- [ ] The `User` record is persisted with the correct `Name`, `Email`, `PasswordHash`, `Role = "Diner"`, and `CreatedAt`.
- [ ] The handler uses `CancellationToken` on both `AnyAsync` and `SaveChangesAsync`.

## Notes

- The handler receives a pre-hashed `PasswordHash` — it does not hash the password itself. BCrypt hashing is the Application layer's responsibility (task-02).
- No repository pattern — `AuthDbContext` is injected directly per CLAUDE.md.
