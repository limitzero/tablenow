# Task 02: Gallery Integration into Restaurant Detail

## Status

pending

## Wave

2

## Description

Adds a photo gallery section to `RestaurantDetailComponent`. Photos fetched from `GET /api/restaurants/{id}` (photos array). No gallery section rendered when photos array is empty.

## Dependencies

**Depends on:** task-01-gallery-component.md, STORY-013 task-03-detail-page.md
**Blocks:** Nothing

**Context from dependencies:**
- task-01 created `PhotoGalleryComponent` with `photos` input of `{url: string}[]`
- Restaurant detail API now includes a `photos` array: `[{ id, url, uploadedAt }]`
- `RestaurantDetailComponent` exists at `features/restaurants/components/restaurant-detail/`

## Files to Modify

- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.ts` — Import and use gallery component
- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.html` — Add gallery section

## Technical Details

Update `Restaurant` model to include photos:
```typescript
export interface Restaurant {
  // ... existing fields
  photos?: Array<{ id: string; url: string; uploadedAt: string }>;
}
```

In detail page HTML:
```html
@if (restaurant.value()?.photos?.length) {
  <section class="photos">
    <h2>Photos</h2>
    <app-photo-gallery [photos]="restaurant.value()!.photos!" />
  </section>
}
```

## Acceptance Criteria

- [ ] Gallery section shown when restaurant has photos
- [ ] Gallery section hidden when `photos` is empty or absent
- [ ] Clicking photo opens lightbox
- [ ] `npm run build` exits with code 0
