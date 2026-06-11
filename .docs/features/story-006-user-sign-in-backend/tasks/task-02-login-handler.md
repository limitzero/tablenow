# Task 02: Login Handler

## Status

pending

## Wave

2

## Description

Creates the Application + Data layers for login: `LoginRequest`, `LoginResponse`, `LoginRequestHandler`, `GetUserByEmailQuery`, and its handler. The Application handler verifies BCrypt credentials and calls `JwtTokenGenerator`. Uses the same 401 message for wrong email and wrong password to prevent oracle attacks.

## Dependencies

**Depends on:** task-01-jwt-helper.md
**Blocks:** task-03-login-endpoint.md

**Context from dependencies:** task-01 created `JwtTokenGenerator` in `Infrastructure/Auth/`. STORY-005 task-01 created `GetUserByEmailQuery` pattern reference (this task creates its own query for login). STORY-003 created the `User` entity and `AppDbContext`. The `Result<T>` type from STORY-001 is available. This task creates all four files from scratch.

## Files to Create

- `server/src/Application/Auth/Features/Login/LoginRequest.cs`
- `server/src/Application/Auth/Features/Login/LoginResponse.cs`
- `server/src/Application/Auth/Features/Login/LoginRequestHandler.cs`
- `server/src/Data/Auth/Queries/GetUserByEmailQuery.cs`
- `server/src/Data/Auth/Queries/GetUserByEmailQueryHandler.cs`

## Technical Details

### Code Snippets

```csharp
// LoginRequest.cs
namespace TableNow.Application.Auth.Features.Login;

public record LoginRequest(string Email, string Password)
    : IRequest<Result<LoginResponse>>;
```

```csharp
// LoginResponse.cs
namespace TableNow.Application.Auth.Features.Login;

public record LoginResponse(string Token, DateTimeOffset ExpiresAt);
```

```csharp
// LoginRequestHandler.cs
namespace TableNow.Application.Auth.Features.Login;

public class LoginRequestHandler(IMediator mediator, JwtTokenGenerator tokenGenerator)
    : IRequestHandler<LoginRequest, Result<LoginResponse>>
{
    private static readonly Result<LoginResponse> InvalidCredentials =
        Result<LoginResponse>.Failure("Invalid credentials.", 401);

    public async ValueTask<Result<LoginResponse>> Handle(
        LoginRequest request, CancellationToken cancellationToken)
    {
        var userResult = await mediator.Send(
            new GetUserByEmailQuery(request.Email), cancellationToken);

        if (!userResult.IsSuccess || userResult.Data is null)
            return InvalidCredentials;

        var user = userResult.Data;
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return InvalidCredentials;

        var (token, expiresAt) = tokenGenerator.Generate(user.Id, user.Email, user.Role);
        return Result<LoginResponse>.Success(new LoginResponse(token, expiresAt));
    }
}
```

```csharp
// GetUserByEmailQuery.cs
namespace TableNow.Data.Auth.Queries;

public record GetUserByEmailQuery(string Email) : IRequest<Result<UserDto>>;
public record UserDto(Guid Id, string Email, string PasswordHash, string Role);
```

```csharp
// GetUserByEmailQueryHandler.cs
namespace TableNow.Data.Auth.Queries;

public class GetUserByEmailQueryHandler(AppDbContext db)
    : IRequestHandler<GetUserByEmailQuery, Result<UserDto>>
{
    public async ValueTask<Result<UserDto>> Handle(
        GetUserByEmailQuery query, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Where(u => u.Email == query.Email)
            .Select(u => new UserDto(u.Id, u.Email, u.PasswordHash, u.Role))
            .FirstOrDefaultAsync(cancellationToken);

        return user is null
            ? Result<UserDto>.Failure("User not found.", 404)
            : Result<UserDto>.Success(user);
    }
}
```

## Acceptance Criteria

- [ ] `LoginRequest`, `LoginResponse`, `LoginRequestHandler` exist in `Application/Auth/Features/Login/`
- [ ] `GetUserByEmailQuery` and handler exist in `Data/Auth/Queries/`
- [ ] Handler returns 401 with "Invalid credentials." for wrong email OR wrong password (same message)
- [ ] Handler returns 200 with `{token, expiresAt}` on valid credentials
- [ ] BCrypt.Verify used for password checking (never plaintext comparison)
