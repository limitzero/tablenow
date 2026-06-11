# Task 02: User Create Data Command

## Status

pending

## Wave

1

## Description

Implements the `CreateUserCommand` / `CreateUserCommandHandler` and `GetUserByEmailQuery` / `GetUserByEmailQueryHandler` in the Data/Auth layer. The command inserts a new `User` row; the query fetches a user by email (used to detect duplicates). All data access is via `AuthDbContext` directly — no repository pattern.

## Dependencies

**Depends on:** STORY-001 task-01-solution-projects.md, STORY-003 task-02-ef-models-configs.md
**Blocks:** task-03-registration-endpoint.md

**Context from dependencies:**
- STORY-001 set up `CM.TableNow.Auth.Data.csproj` with EF Core packages and a project reference to `CM.TableNow.Auth.Domain`
- STORY-003 created `AuthDbContext` with `DbSet<User> Users` and a unique index on `Email`
- `User` entity: `Id (Guid)`, `Name (string)`, `Email (string)`, `PasswordHash (string)`, `Role (string)`, `CreatedAt (DateTime)`

## Files to Create

- `server/src/Data/CM.TableNow.Auth.Data/Commands/CreateUser/CreateUserCommand.cs`
- `server/src/Data/CM.TableNow.Auth.Data/Commands/CreateUser/CreateUserCommandHandler.cs`
- `server/src/Data/CM.TableNow.Auth.Data/Queries/GetUserByEmail/GetUserByEmailQuery.cs`
- `server/src/Data/CM.TableNow.Auth.Data/Queries/GetUserByEmail/GetUserByEmailQueryHandler.cs`

## Files to Modify

None.

## Technical Details

### Code Snippets

```csharp
// CreateUserCommand.cs
namespace CM.TableNow.Auth.Data.Commands.CreateUser;

public record CreateUserCommand(string Name, string Email, string PasswordHash)
    : IRequest<Guid>;
```

```csharp
// CreateUserCommandHandler.cs
using CM.TableNow.Auth.Domain;
using Mediator;

namespace CM.TableNow.Auth.Data.Commands.CreateUser;

public class CreateUserCommandHandler(AuthDbContext db)
    : IRequestHandler<CreateUserCommand, Guid>
{
    public async ValueTask<Guid> Handle(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Email = command.Email,
            PasswordHash = command.PasswordHash,
            Role = "Diner",
            CreatedAt = DateTime.UtcNow,
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return user.Id;
    }
}
```

```csharp
// GetUserByEmailQuery.cs
using CM.TableNow.Auth.Domain;
using Mediator;

namespace CM.TableNow.Auth.Data.Queries.GetUserByEmail;

public record GetUserByEmailQuery(string Email) : IRequest<User?>;
```

```csharp
// GetUserByEmailQueryHandler.cs
using CM.TableNow.Auth.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Auth.Data.Queries.GetUserByEmail;

public class GetUserByEmailQueryHandler(AuthDbContext db)
    : IRequestHandler<GetUserByEmailQuery, User?>
{
    public async ValueTask<User?> Handle(
        GetUserByEmailQuery query,
        CancellationToken cancellationToken)
        => await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == query.Email, cancellationToken);
}
```

## Acceptance Criteria

- [ ] `CreateUserCommand` / `CreateUserCommandHandler` exist at specified paths
- [ ] `GetUserByEmailQuery` / `GetUserByEmailQueryHandler` exist at specified paths
- [ ] Command handler inserts a `User` row and returns the new `Guid`
- [ ] Query handler returns `null` when no user matches the email
- [ ] All handlers use `AuthDbContext` directly (no repository interface)
- [ ] `dotnet build` exits with code 0
