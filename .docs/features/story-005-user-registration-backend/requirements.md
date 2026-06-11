# Requirements: User Registration — Backend

## Summary

New visitors must be able to create an account before making reservations. The registration endpoint accepts name, email, and password; hashes the password with BCrypt; and returns the new user's ID, name, and email. Duplicate email addresses return 409 Conflict. Weak or missing input returns 400 Bad Request with field-level validation errors.

## Goals

- `POST /api/auth/register` returns 201 with `{userId, name, email}` on success
- Duplicate email returns 409 Conflict
- Missing/invalid fields return 400 with validation details
- Password hashed with BCrypt work factor ≥ 12; plaintext never stored

## Non-Goals

- JWT issuance on registration (that's STORY-006 login)
- Email verification flow
- Frontend registration form (STORY-008)

## Acceptance Criteria

- [ ] `POST /api/auth/register` with valid `{name, email, password}` returns 201 and `{userId, name, email}`
- [ ] Duplicate email returns 409 with descriptive error message
- [ ] Missing name/email/password returns 400 with validation details
- [ ] Password with fewer than 8 characters returns 400
- [ ] Password is BCrypt-hashed in the database (plaintext never stored)

## Technical Constraints

- Handler in `Application/Auth/Features/RegisterUser/` following naming: `RegisterUserRequest`, `RegisterUserResponse`, `RegisterUserRequestHandler`
- Data command in `Data/Auth/Commands/CreateUserCommand` + handler
- `Result<T>` return type on all handlers; `TypedResultHelper` at endpoint
- FluentValidation for request validation
- No auth required on this endpoint
