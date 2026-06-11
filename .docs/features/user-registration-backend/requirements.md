# Requirements: User Registration — Backend

## Summary

New visitors register with name, email, and password. The system validates the request, hashes the password with BCrypt (work factor ≥ 12), checks for duplicate email, and creates the user record. Returns 201 with the new user's id, name, and email.

## Goals

- Valid registration returns 201 with userId, name, email
- Password stored as BCrypt hash — plaintext never persisted or logged
- Duplicate email returns 409 Conflict
- Validation failures return 400 with details
- BDD test coverage for the handler

## Non-Goals

- No email verification flow
- No OAuth/social login
- No password-reset flow

## Acceptance Criteria

- [ ] `POST /api/auth/register` with valid data returns 201 with `{ userId, name, email }`
- [ ] Duplicate email returns 409
- [ ] Invalid request (missing name, weak password) returns 400
- [ ] Password hash in database uses BCrypt with work factor ≥ 12
- [ ] BDD tests: `describe_register_user` / `when_email_is_already_taken`, `when_request_is_valid`

## Assumptions

- Minimum password length is 8 characters (validated via FluentValidation)
- No email verification required for MVP

## Technical Constraints

- BCrypt NuGet: `BCrypt.Net-Next`
- Follow Result\<T\> pattern throughout — never throw for business logic failures
- Handler in `Application/Auth/Features/RegisterUser/`
- Data command in `Data/Auth/Commands/CreateUser/`
