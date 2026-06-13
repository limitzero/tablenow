# Task 01: GetUserByEmail Data Query

## Status

pending

## Wave

1

## Description

Implement the Data-layer CQRS query that fetches a user record by email from `AuthDbContext`. `GetUserByEmailQuery` projects the result to a `UserData` record containing only the fields the Application layer needs for login: `Id`, `Email`, `PasswordHash`, and `Role`. A not-found user returns `Result<UserData?>.Success(null)` — the Application layer interprets the null as an authentication failure. This is a read-only, no-tracking query.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-03-login-application.md

**Context from dependencies:** Assumes STORY-001 created the `CM.TableNow.Auth.Data` project with Mediator configured. Assumes STORY-003 created the `User` EF model with `Id`, `Email`, `PasswordHash`, `Role` in `AuthDbContext`. The `UserData` record defined here is the contract that task-03 uses.

## Files to Create

- `server/src/Data/Auth/Queries/GetUserByEmail/GetUserByEmailQuery.cs` — `UserData` record + `GetUserByEmailQuery` record.
- `server/src/Data/Auth/Queries/GetUserByEmail/GetUserByEmailQueryHandler.cs` — Handler projecting to `UserData`.

## Files to Modify

- None.

## Technical Details

### Implementation Steps

1. Define `UserData` as a `record` with `Guid Id`, `string Email`, `string PasswordHash`, `string Role`.
2. Define `GetUserByEmailQuery(string Email)` implementing `IQuery<Result<UserData?>>`.
3. Implement `GetUserByEmailQueryHandler(AuthDbContext db)` with primary constructor.
4. In the handler: `db.Users.AsNoTracking().Where(u => u.Email == query.Email).Select(u => new UserData(u.Id, u.Email, u.PasswordHash, u.Role)).FirstOrDefaultAsync(cancellationToken)`.
5. Return `Result<UserData?>.Success(userData)` — null value is valid (caller handles not-found).

### Code Snippets

```csharp
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Auth.Data.Queries.GetUserByEmail;

public sealed record UserData(Guid Id, string Email, string PasswordHash, string Role);

public sealed record GetUserByEmailQuery(string Email) : IQuery<Result<UserData?>>;
```

```csharp
using CM.TableNow.Shared.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Auth.Data.Queries.GetUserByEmail;

public sealed class GetUserByEmailQueryHandler(AuthDbContext db)
    : IQueryHandler<GetUserByEmailQuery, Result<UserData?>>
{
    public async ValueTask<Result<UserData?>> Handle(
        GetUserByEmailQuery query,
        CancellationToken cancellationToken)
    {
        var data = await db.Users
            .AsNoTracking()
            .Where(u => u.Email == query.Email)
            .Select(u => new UserData(u.Id, u.Email, u.PasswordHash, u.Role))
            .FirstOrDefaultAsync(cancellationToken);

        return Result<UserData?>.Success(data);
    }
}
```

## Acceptance Criteria

- [ ] Query returns `Result<UserData?>.Success(userData)` with the correct fields when the email exists.
- [ ] Query returns `Result<UserData?>.Success(null)` when the email does not exist.
- [ ] Query uses `AsNoTracking` (read-only).
- [ ] Projection to `UserData` is done server-side in EF LINQ, not in memory.

## Notes

- Returning `Success(null)` (rather than a failure result) for a not-found user is intentional: it lets the Application layer run the BCrypt dummy comparison regardless, preventing timing-based user enumeration.
