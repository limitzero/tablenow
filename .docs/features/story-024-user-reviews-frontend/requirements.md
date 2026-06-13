# Requirements: User Reviews — Frontend

## Summary

Diners want to read reviews when browsing a restaurant and share their own experience after dining. This feature extends the restaurant detail page with a review list and, for authenticated users, a review submission form with a star rating input and text area. Unauthenticated visitors see only the read-only list.

## Goals

- Restaurant detail page shows existing reviews with author, stars, body, timestamp.
- Authenticated users see a review submission form.
- Submitted review appears in the list immediately (optimistic update or re-fetch).
- Unauthenticated users see only the list, no submission form.

## Non-Goals

- No editing or deleting reviews.
- No pagination controls (use default pageSize from API).
- No helpful/unhelpful voting.

## Acceptance Criteria

- [ ] Review list shows `authorName`, `rating` (stars), `body`, `createdAt` for each review.
- [ ] Authenticated users see a submission form.
- [ ] Submitting a valid review adds it to the list.
- [ ] Unauthenticated users do not see the form.

## Technical Constraints

- Add review section to `features/restaurants/` detail component.
- Use `httpResource()` for fetching reviews.
- Star rating: Angular Material's star rating or a simple custom component using `@for`.
- Check `isAuthenticated` signal from `AuthService` to conditionally render the form.
- `OnPush` change detection; no `.subscribe()` in components.
