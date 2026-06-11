# Task 05: User Registration Backend

## Status

pending

## Phase

3

## Description

Implement the `POST /api/auth/register` endpoint using the CQRS/Mediator pattern. A new visitor provides their name, email, and password. The Application handler validates the input, checks for duplicate emails, hashes the password with BCrypt, and creates the user record. The endpoint returns 201 with the new user's id, name, and email. Returns 409 on duplicate email and 400 on validation failure.

## Dependencies

**Depends on:** task-01-backend-scaffolding, task-03-database-schema  
**Blocks:** task-06-user-signin-jwt-backend

**Context from dependencies:** task-01 created the `TableNow.Auth.Application`, `TableNow.Auth.Domain`, `TableNow.Auth.Data`, and `TableNow.Api` projects with the `Result<T>` / `TypedResultHelper` pattern. task-03 created `AppDbContext` with `DbSet<UserModel>` and a `UserModel` entity with `Id`, `Name`, `Email`, `PasswordHash`, `Role`, and `CreatedAt` fields. `Email` has a unique index.

## Files to Create

- `server/src/Application/Auth/Features/RegisterUser/RegisterUserRequest.cs` — application request record
- `server/src/Application/Auth/Features/RegisterUser/RegisterUserResponse.cs` — application response record
- `server/src/Application/Auth/Features/RegisterUser/RegisterUserRequestHandler.cs` — orchestrates validation + data command
- `server/src/Application/Auth/Features/RegisterUser/RegisterUserValidator.cs` — FluentValidation rules
- `server/src/Data/Auth/Commands/CreateUser/CreateUserCommand.cs` — data command record
- `server/src/Data/Auth/Commands/CreateUser/CreateUserCommandHandler.cs` — writes to DbContext
- `server/src/Data/Auth/Queries/GetUserByEmail/GetUserByEmailQuery.cs` — data query record
- `server/src/Data/Auth/Queries/GetUserByEmail/GetUserByEmailQueryHandler.cs` — reads from DbContext
- `server/src/Api/Auth/AuthEndpoints.cs` — static class with `MapAuthEndpoints(RouteGroupBuilder)`
- `server/src/Api/Auth/AuthMapper.cs` — static mapper: API model ↔ application request/response
- `server/src/Contracts/Auth/RegisterRequest.cs` — API-facing request DTO
- `server/src/Contracts/Auth/RegisterResponse.cs` — API-facing response DTO
- `server/src/Application/Auth/AuthModule.cs` — `AddAuthModule()` extension registering Mediator handlers

## Files to Modify

- `server/src/Api/ServiceCollectionExtensions.cs` — call `services.AddAuthModule()`
- `server/src/Api/Program.cs` — call `app.MapGroup("api/v1").MapAuthEndpoints()`
- `server/src/Data/Auth/TableNow.Auth.Data.csproj` — add `BCrypt.Net-Next` package

## Technical Details

### Implementation Steps

1. **Add BCrypt package to Auth.Data:**
   ```powershell
   dotnet add src/Data/Auth package BCrypt.Net-Next
   ```

2. **Add Mediator source-generation to Application projects:**
   ```powershell
   dotnet add src/Application/Auth package Mediator
   dotnet add src/Data/Auth package Mediator
   ```

3. **Write Contracts DTOs** (in `TableNow.Contracts`).

4. **Write Application request/response records** (in `Application/Auth/Features/RegisterUser/`).

5. **Write FluentValidation validator** for the request.

6. **Write Data query** to check if email already exists.

7. **Write Data command handler** to create the user (hashes password with BCrypt).

8. **Write Application handler** that: validates → checks duplicate → creates user.

9. **Write static `AuthMapper`** for API ↔ Application translation.

10. **Write `AuthEndpoints`** with `POST /register` route.

11. **Register module** and wire endpoints in `Program.cs`.

### Code Snippets

**Contracts (API-facing DTOs):**
```csharp
// src/Contracts/Auth/RegisterRequest.cs
namespace TableNow.Contracts.Auth;
public sealed record RegisterRequest(string Name, string Email, string Password);

// src/Contracts/Auth/RegisterResponse.cs
namespace TableNow.Contracts.Auth;
public sealed record RegisterResponse(Guid UserId, string Name, string Email);
```

**Application request/response:**
```csharp
// Application/Auth/Features/RegisterUser/RegisterUserRequest.cs
namespace TableNow.Auth.Application.Features.RegisterUser;
public sealed record RegisterUserRequest(string Name, string Email, string Password)
    : IRequest<Result<RegisterUserResponse>>;

public sealed record RegisterUserResponse(Guid UserId, string Name, string Email);
```

**FluentValidation validator:**
```csharp
public sealed class RegisterUserValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
```

**Application handler:**
```csharp
namespace TableNow.Auth.Application.Features.RegisterUser;

public sealed class RegisterUserRequestHandler(IMediator mediator)
    : IRequestHandler<RegisterUserRequest, Result<RegisterUserResponse>>
{
    public async ValueTask<Result<RegisterUserResponse>> Handle(
        RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var validator = new RegisterUserValidator();
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<RegisterUserResponse>.Failure(400,
                validation.Errors.Select(e => e.ErrorMessage).ToArray());

        var existing = await mediator.Send(
            new GetUserByEmailQuery(request.Email), cancellationToken);
        if (existing is not null)
            return Result<RegisterUserResponse>.Failure(409, "Email is already registered.");

        var result = await mediator.Send(
            new CreateUserCommand(request.Name, request.Email, request.Password),
            cancellationToken);

        return Result<RegisterUserResponse>.Success(
            new RegisterUserResponse(result.UserId, result.Name, result.Email), 201);
    }
}
```

**Data command (CreateUser):**
```csharp
namespace TableNow.Auth.Data.Commands.CreateUser;

public sealed record CreateUserCommand(string Name, string Email, string Password)
    : IRequest<CreateUserCommandResult>;

public sealed record CreateUserCommandResult(Guid UserId, string Name, string Email);

public sealed class CreateUserCommandHandler(AppDbContext db)
    : IRequestHandler<CreateUserCommand, CreateUserCommandResult>
{
    public async ValueTask<CreateUserCommandResult> Handle(
        CreateUserCommand command, CancellationToken cancellationToken)
    {
        var user = new UserModel
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Email = command.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.Password, workFactor: 12),
            Role = "diner",
            CreatedAt = DateTime.UtcNow,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return new CreateUserCommandResult(user.Id, user.Name, user.Email);
    }
}
```

**Data query (GetUserByEmail):**
```csharp
public sealed record GetUserByEmailQuery(string Email) : IRequest<UserModel?>;

public sealed class GetUserByEmailQueryHandler(AppDbContext db)
    : IRequestHandler<GetUserByEmailQuery, UserModel?>
{
    public async ValueTask<UserModel?> Handle(
        GetUserByEmailQuery query, CancellationToken cancellationToken)
        => await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == query.Email, cancellationToken);
}
```

**`AuthEndpoints.cs`:**
```csharp
namespace TableNow.Api.Auth;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/auth/register", async (
            RegisterRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(
                AuthMapper.ToRegisterRequest(request), ct);
            return TypedResultHelper.ToResult(result);
        });

        // Login endpoint added in task-06
        return group;
    }
}
```

**`AuthMapper.cs`:**
```csharp
namespace TableNow.Api.Auth;

public static class AuthMapper
{
    public static RegisterUserRequest ToRegisterRequest(RegisterRequest r) =>
        new(r.Name, r.Email, r.Password);
}
```

**API response shape:**
```json
// POST /api/v1/auth/register — 201
{ "userId": "uuid", "name": "Jane Doe", "email": "jane@example.com" }

// 409
{ "errors": ["Email is already registered."] }

// 400
{ "errors": ["'Email' must be a valid email address.", "'Password' minimum length is 8."] }
```

## Acceptance Criteria

- [ ] `POST /api/v1/auth/register` with valid data returns 201 with `userId`, `name`, `email`
- [ ] Password is stored as a BCrypt hash (work factor 12); plaintext never in the database
- [ ] Duplicate email returns 409 with a descriptive error message
- [ ] Missing or invalid fields return 400 with field-level validation errors
- [ ] `dotnet build` passes after all files are added

## Notes

- The CQRS flow is: `AuthEndpoints` → `IMediator.Send(RegisterUserRequest)` → `RegisterUserRequestHandler` → `IMediator.Send(GetUserByEmailQuery)` + `IMediator.Send(CreateUserCommand)`.
- Use `BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12)` — never 10 or lower in production.
- The `GetUserByEmail` query must use `AsNoTracking()` — it is read-only.
- File-scoped namespaces, primary constructors for DI, records for all DTOs/commands/queries.
- BDD unit tests (added in a separate testing task if requested): `namespace describe_register_user`, classes `when_email_is_already_taken`, `when_request_is_valid`.
