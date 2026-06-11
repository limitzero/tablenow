# Requirements: Photo Gallery — Frontend

## Summary

A photo gallery on the restaurant detail page lets diners preview food and ambiance. Photos lazy-load to avoid slowing page load. Clicking a photo opens an enlarged view. No section shown when photos is empty.

## Goals

- Photo grid shown when restaurant has photos
- No gallery section when photos array is empty
- Photos use `loading="lazy"`
- Click → lightbox/enlarged view (MatDialog)
- Skeleton placeholders while loading

## Acceptance Criteria

- [ ] Gallery visible only when photos.length > 0
- [ ] Skeleton shown while loading
- [ ] Photos use loading="lazy"
- [ ] Clicking opens enlarged view

## Technical Constraints

- Component in `features/restaurants/components/`
- MatDialog for lightbox
- OnPush
