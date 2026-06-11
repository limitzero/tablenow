# Task 01: Photo Gallery Component

## Status

pending

## Wave

1

## Description

Creates `PhotoGalleryComponent` that accepts a `photos` input array, renders a lazy-loaded grid, and opens a MatDialog lightbox on photo click.

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-002 Angular scaffold)
**Blocks:** task-02-gallery-integration.md

## Files to Create

- `client/src/app/features/restaurants/components/photo-gallery.component.ts`
- `client/src/app/features/restaurants/models/photo.model.ts`

## Technical Details

### Code Snippets

```typescript
// models/photo.model.ts
export interface PhotoDto { id: string; url: string; uploadedAt: string; }
```

```typescript
// photo-gallery.component.ts
@Component({
  selector: 'app-photo-gallery',
  standalone: true,
  imports: [MatGridListModule, MatDialogModule, MatProgressSpinnerModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (photos().length > 0) {
      <div class="gallery-section">
        <h3>Photos</h3>
        <div class="photo-grid">
          @for (photo of photos(); track photo.id) {
            <img [src]="photo.url" loading="lazy"
                 (click)="openLightbox(photo)"
                 class="gallery-thumb"
                 alt="Restaurant photo" />
          }
        </div>
      </div>
    }
  `,
})
export class PhotoGalleryComponent {
  readonly photos = input.required<PhotoDto[]>();
  private readonly dialog = inject(MatDialog);

  openLightbox(photo: PhotoDto): void {
    this.dialog.open(PhotoLightboxDialogComponent, {
      data: photo,
      maxWidth: '90vw',
      maxHeight: '90vh',
    });
  }
}

@Component({
  selector: 'app-photo-lightbox-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule],
  template: `
    <img [src]="photo.url" style="max-width:100%;max-height:80vh;" alt="Photo" />
    <mat-dialog-actions>
      <button mat-button mat-dialog-close>Close</button>
    </mat-dialog-actions>
  `,
})
export class PhotoLightboxDialogComponent {
  protected readonly photo = inject<PhotoDto>(MAT_DIALOG_DATA);
}
```

## Acceptance Criteria

- [ ] Gallery section only rendered when `photos().length > 0`
- [ ] Each image uses `loading="lazy"`
- [ ] Click opens `PhotoLightboxDialogComponent` with enlarged image
- [ ] OnPush, standalone
