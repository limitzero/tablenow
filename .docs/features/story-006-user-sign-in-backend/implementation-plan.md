# Implementation Plan: User Sign-In — Backend

## Phase 1 — Data query and JWT infrastructure (parallel)

task-01 (Data project) and task-02 (Infrastructure project) are fully independent and can be implemented simultaneously.

- [ ] **task-01-get-user-by-email-query** — `GetUserByEmailQuery(Email)` + `UserData` projection record + `GetUserByEmailQueryHandler` that fetches the user by email from `AuthDbContext` and returns `Result<UserData?>`.
- [ ] **task-02-jwt-token-generator** — `JwtOptions` record + `JwtTokenGenerator` class in `Infrastructure/Auth/` that generates a signed HS256 JWT with `userId`, `email`, and `role` claims.

### Technical Details

**Data query** — `GetUserByEmailQuery`:
```csharp
public sealed record UserData(Guid Id, string Email, string PasswordHash, string Role);
public sealed record GetUserByEmailQuery(string Email) : IQuery<Result<UserData?>>;
```
Handler: `db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == query.Email)` → project to `UserData`; return `Result<UserData?>.Success(data)` (null data means not found — handler returns success with null, caller interprets).

**JWT generator** — `JwtTokenGenerator`:
```csharp
public string GenerateToken(Guid userId, string email, string role);
```
Uses `IOptions<JwtOptions>`, creates `JwtSecurityToken` with claims `sub=userId`, `email=email`, `role=role`, `jti=Guid.NewGuid()`, signs with `HS256`, returns serialized string and `expiresAt`.

## Phase 2 — Application layer

Runs after both Phase 1 tasks are complete (needs `UserData` contract and `JwtTokenGenerator`).

- [ ] **task-03-login-application** — `LoginRequest(Email, Password)` + `LoginResponse(Token, ExpiresAt)` + `LoginRequestHandler` that looks up the user, runs BCrypt compare, calls `JwtTokenGenerator`, and returns `Result<LoginResponse>`.

### Technical Details

Handler steps: fetch user → if null, BCrypt compare against dummy hash, return `Result.Failure(401, "Invalid credentials")` → if found but password wrong, return same 401 → on success, call `JwtTokenGenerator.GenerateToken(...)`, return `Result<LoginResponse>.Success(...)`.

## Phase 3 — Endpoint and BDD tests

- [ ] **task-04-login-endpoint** — `POST /api/auth/login` Minimal API endpoint + BDD tests (`describe_login` with `when_credentials_are_valid` and `when_credentials_are_invalid`).
