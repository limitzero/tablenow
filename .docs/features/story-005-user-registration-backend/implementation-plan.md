# Implementation Plan: User Registration — Backend

## Phase 1 — Data and Application layers (parallel)

task-01 and task-02 live in different projects (`Data/Auth` vs `Application/Auth`) and do not share files, so they can be implemented simultaneously.

- [ ] **task-01-create-user-command** — `CreateUserCommand(Name, Email, PasswordHash)` record + `CreateUserCommandHandler` that checks for duplicate email in `AuthDbContext`, inserts the `User` record, and returns `Result<Guid>` (the new `userId`).
- [ ] **task-02-register-user-application** — `RegisterUserRequest(Name, Email, Password)` + `RegisterUserRequestHandler` that validates input (min 8-char password), hashes the password with BCrypt, dispatches `CreateUserCommand`, and returns `Result<RegisterUserResponse>`.

### Technical Details

**Data layer** — `CreateUserCommand`:
```csharp
public sealed record CreateUserCommand(string Name, string Email, string PasswordHash)
    : ICommand<Result<Guid>>;
```
Handler: check `db.Users.AnyAsync(u => u.Email == command.Email)` → return `Result<Guid>.Failure(409, "Email already registered")` if taken; else `db.Users.Add(user)`, `SaveChangesAsync`, return `Result<Guid>.Success(user.Id)`.

**Application layer** — `RegisterUserRequest`:
```csharp
public sealed record RegisterUserRequest(string Name, string Email, string Password)
    : IRequest<Result<RegisterUserResponse>>;
public sealed record RegisterUserResponse(Guid UserId, string Name, string Email);
```
Handler: validate (name not empty, valid email format, password ≥ 8 chars) → hash with `BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12)` → dispatch `CreateUserCommand` → map to `RegisterUserResponse`.

## Phase 2 — Endpoint and BDD tests

Runs after both Phase 1 tasks are complete.

- [ ] **task-03-register-endpoint** — Minimal API `POST /api/auth/register` endpoint, `AuthMapper` static class for request → command mapping, and BDD tests (`describe_register_user` with `when_email_is_already_taken` and `when_request_is_valid`).

### Technical Details

Endpoint maps `RegisterRequest` (API contract) → `RegisterUserRequest` via `AuthMapper`, sends through `IMediator`, and calls `TypedResultHelper.ToResult(result)` to get the correct `IResult`.

BDD tests use `module_fixture` (mocked `IMediator`) for handler unit tests. Namespace: `describe_register_user`.
