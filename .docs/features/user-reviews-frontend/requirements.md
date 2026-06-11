# Requirements: User Reviews — Frontend

## Summary

Adds reviews section to restaurant detail page. Authenticated users see a star rating + text form. Submitted review appears immediately. Unauthenticated visitors see reviews but no form.

## Goals

- Reviews list with author, stars, text, timestamp
- Submission form visible only to authenticated users
- New review appears immediately after submit

## Acceptance Criteria

- [ ] Reviews shown on restaurant detail page
- [ ] Authenticated user sees submission form
- [ ] Unauthenticated visitor does not see form
- [ ] New review appears in list after submit

## Technical Constraints

- Add to `features/restaurants/` detail component
- Star rating: custom component or Angular Material rating
- Conditionally render form based on `isAuthenticated()` signal
