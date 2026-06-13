# Requirements: Photo Gallery — Frontend

## Summary

Diners want to see food and ambiance photos on a restaurant's detail page before booking. This feature adds a photo gallery section that shows photos from the `photos` array in the restaurant detail API response. Images are lazy-loaded; clicking opens a lightbox. Skeleton placeholders are shown during load. If no photos are present, the section is hidden.

## Goals

- Photo gallery section shown when restaurant has photos.
- Images use `loading="lazy"`.
- Clicking a photo opens an enlarged/lightbox view.
- Skeleton placeholders shown during loading.
- Section hidden if no photos.

## Non-Goals

- No upload UI for diners — that is operator-only (STORY-025).
- No carousel auto-play.
- No video support.

## Acceptance Criteria

- [ ] Gallery section shows photos when available.
- [ ] No gallery section shown when `photos` array is empty.
- [ ] Clicking a photo opens enlarged view.
- [ ] Skeleton placeholders visible during load.

## Technical Constraints

- Add to `features/restaurants/` detail component.
- Use Angular Material dialog or a custom overlay for lightbox.
- `loading="lazy"` on all `<img>` elements.
- `OnPush` change detection.
