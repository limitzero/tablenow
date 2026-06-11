# Requirements: Photo Gallery — Frontend

## Summary

Adds a photo gallery to the restaurant detail page with lazy-loaded images. Clicking a photo opens a lightbox. Skeleton placeholders during loading. No gallery section if no photos.

## Goals

- Gallery shown when restaurant has photos
- Lazy-loaded images
- Lightbox on click
- Skeleton placeholders during load
- No section when no photos

## Acceptance Criteria

- [ ] Gallery shown with photos
- [ ] No gallery section when `photos` array is empty
- [ ] Clicking photo opens lightbox
- [ ] Images load lazily

## Technical Constraints

- Add to `features/restaurants/` detail component
- `loading="lazy"` attribute on all images
