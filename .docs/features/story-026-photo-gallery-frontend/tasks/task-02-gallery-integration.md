# Task 02: Gallery Integration

## Status

pending

## Wave

2

## Description

Adds `PhotoGalleryComponent` to `RestaurantDetailComponent`. Passes the `photos` array from the restaurant detail API response.

## Dependencies

**Depends on:** task-01-photo-gallery-component.md
**Blocks:** Nothing

**Context from dependencies:** task-01 created `PhotoGalleryComponent` accepting `photos` input. STORY-013 created `RestaurantDetailComponent` with `restaurantResource`. STORY-025 task-01 updated `RestaurantDto` to include a `photos` array.

## Files to Modify

- `client/src/app/features/restaurants/components/restaurant-detail.component.ts`
- `client/src/app/features/restaurants/models/restaurant.model.ts` — add `photos` to `RestaurantDto`

## Technical Details

```typescript
// Update RestaurantDto model:
export interface RestaurantDto {
  id: string;
  name: string;
  cuisine: string;
  address: string;
  description: string;
  thumbnailUrl: string;
  photos: PhotoDto[]; // NEW
}
```

```html
<!-- Add to restaurant-detail template: -->
<app-photo-gallery [photos]="restaurantResource.value()?.photos ?? []" />
```

```typescript
// Add to imports array of RestaurantDetailComponent:
PhotoGalleryComponent,
```

## Acceptance Criteria

- [ ] `PhotoGalleryComponent` added to `RestaurantDetailComponent` template
- [ ] `RestaurantDto` includes `photos: PhotoDto[]`
- [ ] Gallery receives photos from `restaurantResource.value()?.photos ?? []`
- [ ] No gallery visible when photos array is empty (handled in task-01)
