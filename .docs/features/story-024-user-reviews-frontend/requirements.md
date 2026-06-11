# Requirements: User Reviews — Frontend

## Summary

Reviews section on the restaurant detail page showing existing reviews and (for authenticated users) a submission form. New reviews appear immediately after submission.

## Goals

- Review list: author, star rating, body, timestamp
- Submission form: star rating (1–5) + text; shown only to authenticated users
- After submission: review appears in list immediately

## Acceptance Criteria

- [ ] Reviews listed with author name, stars, text, date
- [ ] Form only shown when `isAuthenticated`
- [ ] After submission, review appears in list
- [ ] Unauthenticated visitors see reviews but no form

## Technical Constraints

- Components in `features/restaurants/components/`
- Read `isAuthenticated` from `AuthService` signal
- OnPush on all components
