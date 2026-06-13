# Task 01: Photo Gallery Component

## Status

pending

## Wave

1

## Description

Create a standalone `PhotoGalleryComponent` that accepts a `photos` input signal (array of photo objects with `id`, `url`, `uploadedAt`). Renders an image grid with `loading="lazy"`. Shows skeleton placeholders via `@if (isLoading)`. Hidden entirely when photos array is empty. Add the component to the restaurant detail template.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-02-lightbox-view.md

## Files to Create

- `client/src/app/features/restaurants/components/photo-gallery/photo-gallery.component.ts`
- `client/src/app/features/restaurants/components/photo-gallery/photo-gallery.component.html`
- `client/src/app/features/restaurants/models/photo.model.ts` — `Photo` interface.

## Files to Modify

- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.html` — Add `<app-photo-gallery>`.
- `client/src/app/features/restaurants/index.ts` — Export `PhotoGalleryComponent`.

## Technical Details

```typescript
export interface Photo { id: string; url: string; uploadedAt: string; }

@Component({
  selector: 'app-photo-gallery',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatProgressSpinnerModule],
  templateUrl: './photo-gallery.component.html',
})
export class PhotoGalleryComponent {
  readonly photos = input<Photo[]>([]);
  readonly isLoading = input(false);
  // onPhotoClick output added by task-02
}
```

```html
@if (photos().length > 0) {
  <section class="photo-gallery">
    <h3>Photos</h3>
    <div class="photo-grid">
      @if (isLoading()) {
        @for (n of [1,2,3,4,5,6]; track n) {
          <div class="photo-skeleton"></div>
        }
      } @else {
        @for (photo of photos(); track photo.id) {
          <img [src]="photo.url" [alt]="'Restaurant photo'" loading="lazy" class="gallery-img" />
        }
      }
    </div>
  </section>
}
```

## Acceptance Criteria

- [ ] Gallery section hidden when `photos()` is empty.
- [ ] Images use `loading="lazy"`.
- [ ] Skeleton placeholders shown when `isLoading()` is true.
- [ ] `OnPush` change detection.
