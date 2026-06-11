# Task 01: Photo Gallery Component

## Status

pending

## Wave

1

## Description

Creates a standalone `PhotoGalleryComponent` displaying a grid of lazy-loaded images. Clicking an image opens a simple lightbox (full-screen overlay). Shows skeleton placeholders during loading.

## Dependencies

**Depends on:** STORY-002 task-03-environment-config.md
**Blocks:** task-02-gallery-integration.md

## Files to Create

- `client/src/app/shared/components/photo-gallery/photo-gallery.component.ts`
- `client/src/app/shared/components/photo-gallery/photo-gallery.component.html`
- `client/src/app/shared/components/photo-gallery/photo-gallery.component.scss`

## Technical Details

```typescript
@Component({
  selector: 'app-photo-gallery',
  standalone: true,
  imports: [MatProgressSpinnerModule],
  template: `
    <div class="gallery-grid">
      @for (photo of photos(); track photo.url) {
        <div class="gallery-item">
          <img [src]="photo.url" [alt]="photo.url" loading="lazy"
               (click)="openLightbox(photo.url)" />
        </div>
      }
    </div>

    @if (lightboxUrl()) {
      <div class="lightbox" (click)="closeLightbox()">
        <img [src]="lightboxUrl()!" alt="Photo" />
      </div>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PhotoGalleryComponent {
  photos = input.required<Array<{ url: string }>>();
  lightboxUrl = signal<string | null>(null);

  openLightbox(url: string): void { this.lightboxUrl.set(url); }
  closeLightbox(): void { this.lightboxUrl.set(null); }
}
```

## Acceptance Criteria

- [ ] Gallery grid renders photos with `loading="lazy"`
- [ ] Clicking opens lightbox overlay
- [ ] Clicking lightbox closes it
- [ ] `changeDetection: OnPush`
- [ ] `npm run build` exits with code 0
