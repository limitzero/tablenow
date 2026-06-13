# Requirements: User Registration — Backend

## Summary

TableNow needs a user registration endpoint so that new visitors can create an account before making reservations. The endpoint accepts name, email, and password; validates the input; checks for duplicate emails; and persists the BCrypt-hashed password. A 201 Created response with `userId`, `name`, and `email` is returned on success.

Following the project's modular monolith CQRS architecture, the request flows from a Minimal API endpoint → `RegisterUserRequest` (Application) → `CreateUserCommand` (Data) → `AuthDbContext`. The `Result<T>` pattern is used throughout; the endpoint delegates HTTP status mapping to `TypedResultHelper`.

The expected outcome is a working `/api/auth/register` endpoint with BDD unit tests covering the happy path and the duplicate-email failure case.

## Goals

- `POST /api/auth/register` accepts `{ name, email, password }` and returns 201 `{ userId, name, email }`.
- Duplicate email returns 409 Conflict with a descriptive error message.
- Invalid or missing fields return 400 Bad Request with validation details.
- Password is hashed with BCrypt (work factor ≥ 12); plaintext is never stored or logged.
- BDD tests: `describe_register_user` with `when_email_is_already_taken` and `when_request_is_valid` classes.

## Non-Goals

- No email verification flow — registration is immediate.
- No JWT issuance on registration — that is handled by the Login endpoint (STORY-006).
- No role assignment beyond the default `"Diner"` role.
- No password strength validator beyond minimum length.

## Acceptance Criteria

- [ ] `POST /api/auth/register` with valid name/email/password returns 201 with `userId`, `name`, and `email`.
- [ ] A duplicate email returns 409 Conflict.
- [ ] Missing or invalid fields return 400 Bad Request with validation details.
- [ ] The stored `PasswordHash` is a BCrypt hash (not plaintext).
- [ ] BDD tests pass for the happy path and duplicate-email case.

## Assumptions

- STORY-001 created the `CM.TableNow.Auth.Application`, `CM.TableNow.Auth.Data`, and `CM.TableNow.Api` projects.
- STORY-003 created the `User` EF model and `AuthDbContext`.
- `Result<T>` from `CM.TableNow.Shared` is available with `StatusCode`, `IsSuccess`, `Data`, and `Errors` members.
- `TypedResultHelper` in the Api project maps `Result<T>.StatusCode` to the correct `IResult` type.

## Technical Constraints

- Handler: `RegisterUserRequest` / `RegisterUserResponse` / `RegisterUserRequestHandler` in `Application/Auth/Features/RegisterUser/`.
- Data command: `CreateUserCommand` / `CreateUserCommandHandler` in `Data/Auth/Commands/CreateUser/`.
- Password hashing: `BCrypt.Net-Next` package, work factor ≥ 12.
- No repository pattern — `AuthDbContext` is used directly in the Data command handler.
- File-scoped namespaces, nullable enabled, records for DTOs.
- `CancellationToken` on all async methods.
