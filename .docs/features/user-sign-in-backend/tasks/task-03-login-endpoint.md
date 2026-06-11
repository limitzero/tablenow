# Task 03: Login Endpoint

## Status

pending

## Wave

3

## Description

Adds `POST /api/auth/login` to `AuthEndpoints` and calls `AddAuthInfrastructure()` from the Auth module registration. Adds a contract type for the login API request.

## Dependencies

**Depends on:** task-02-login-handler.md
**Blocks:** STORY-007 (JWT middleware must match the token format produced here), STORY-008 (frontend needs the login endpoint)

**Context from dependencies:**
- task-02 created `LoginRequest` and `LoginResponse` in `Application/Auth/Features/Login/`
- STORY-005 task-03 created `AuthEndpoints.cs` with the register endpoint already mapped — this task adds the login endpoint to the same file
- The `AddAuthModule()` extension method was created in STORY-005 task-03 — this task adds `AddAuthInfrastructure()` to it

## Files to Create

- `server/src/Contracts/Auth/LoginApiRequest.cs` — API-facing request model

## Files to Modify

- `server/src/Api/Endpoints/AuthEndpoints.cs` — Add `POST /auth/login` route
- `server/src/Application/CM.TableNow.Auth.Application/Extensions/ServiceCollectionExtensions.cs` — Add `services.AddAuthInfrastructure(configuration)`

## Technical Details

### Code Snippets

```csharp
// Contracts/Auth/LoginApiRequest.cs
namespace CM.TableNow.Contracts;

public record LoginApiRequest(string Email, string Password);
```

Add to `AuthEndpoints.cs` inside `MapAuthEndpoints`:
```csharp
auth.MapPost("/login", async (
    LoginApiRequest apiRequest,
    IMediator mediator,
    CancellationToken ct) =>
{
    var result = await mediator.Send(new LoginRequest(apiRequest.Email, apiRequest.Password), ct);
    return TypedResultHelper.ToResult(result);
})
.WithName("Login")
.Produces<LoginResponse>(200)
.ProducesProblem(401);
```

Update `AddAuthModule` in `Application/Auth/Application/Extensions/ServiceCollectionExtensions.cs`:
```csharp
public static IServiceCollection AddAuthModule(
    this IServiceCollection services,
    IConfiguration configuration)
{
    services.AddAuthInfrastructure(configuration);
    // Mediator handlers are auto-discovered via source generation
    return services;
}
```

## Acceptance Criteria

- [ ] `POST /api/auth/login` with valid credentials returns 200 with `{ token, expiresAt }`
- [ ] `POST /api/auth/login` with wrong password returns 401
- [ ] `POST /api/auth/login` with unknown email returns 401
- [ ] Decoded JWT contains `sub` (userId), `email`, and `role` claims
- [ ] `dotnet build` exits with code 0
