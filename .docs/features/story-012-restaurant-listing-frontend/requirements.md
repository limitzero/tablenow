# Requirements: Restaurant Listing — Frontend

## Summary

Diners need a browsable grid of restaurants as the main discovery surface of TableNow. This feature adds the `/restaurants` Angular route that fetches all restaurants from `GET /api/restaurants` and renders them as Material cards showing name, cuisine, and address. A client-side cuisine filter dropdown narrows the list without additional API calls. Clicking a card navigates to the detail route `/restaurants/:id`.

State (restaurant list, cuisine filter selection) is owned by an NgRx Signal Store slice. Data is fetched via `httpResource()`. All components use `OnPush` change detection. No `.subscribe()` calls in components.

## Goals

- `/restaurants` route renders a responsive grid of restaurant cards.
- Each card shows name, cuisine, and address/neighborhood.
- Cuisine filter control narrows the displayed list client-side.
- Clicking a card navigates to `/restaurants/:id`.
- Page load completes in < 2s on a standard connection.

## Non-Goals

- No search by name — only cuisine filter.
- No pagination — all restaurants are loaded at once (MVP scope).
- No authentication required on the listing page — it is public.
- No restaurant detail content on this page — that is STORY-013.

## Acceptance Criteria

- [ ] `/restaurants` displays a grid of restaurant cards fetched from the API.
- [ ] Each card shows name, cuisine, and address/neighborhood.
- [ ] The cuisine filter control narrows results client-side.
- [ ] Clicking a card navigates to `/restaurants/:id`.
- [ ] The route is accessible without authentication.

## Assumptions

- STORY-009 (JWT interceptor) is complete so the HTTP interceptor is available, but this route does not require authentication.
- STORY-010 (`GET /api/restaurants`) is deployed and returning restaurant data.
- Angular Material is installed and themed per STORY-002.
- The `environment.ts` `apiBaseUrl` points to `http://localhost:5000/api`.

## Technical Constraints

- Feature folder: `client/src/app/features/restaurants/`.
- NgRx Signal Store slice in `store/restaurants.store.ts` — holds `restaurants` signal and `cuisineFilter` signal.
- `httpResource()` for API calls — no `HttpClient.get().subscribe()` in components.
- `OnPush` change detection on all components.
- `@for` / `@if` template control flow — no `*ngFor` / `*ngIf`.
- `inject()` for DI — no constructor injection.
- Barrel export via `index.ts` in the feature folder.
