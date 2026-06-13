# Task 02: Lightbox / Enlarged View

## Status

pending

## Wave

2

## Description

Add click-to-enlarge functionality to the photo gallery. Clicking an image opens a Material Dialog showing the full-size photo with the restaurant name as the title. Add an `output` event to `PhotoGalleryComponent` and wire it to the dialog in the parent detail component.

## Dependencies

**Depends on:** task-01-photo-gallery-component.md
**Blocks:** None

**Context from dependencies:** task-01 created `PhotoGalleryComponent` with photo grid. This task adds `(click)` events to each `<img>` and opens an `MatDialog` with the selected photo URL.

## Files to Create

- `client/src/app/features/restaurants/components/photo-gallery/photo-lightbox-dialog.component.ts` — Simple dialog component showing a full-size image.

## Files to Modify

- `client/src/app/features/restaurants/components/photo-gallery/photo-gallery.component.ts` — Add `photoClicked = output<Photo>()`.
- `client/src/app/features/restaurants/components/photo-gallery/photo-gallery.component.html` — Add `(click)="photoClicked.emit(photo)"` to each `<img>`.
- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.ts` — Inject `MatDialog`, open `PhotoLightboxDialogComponent` on `(photoClicked)`.

## Technical Details

```typescript
// photo-lightbox-dialog.component.ts
@Component({
  selector: 'app-photo-lightbox-dialog',
  standalone: true,
  imports: [MatDialogModule],
  template: `
    <mat-dialog-content>
      <img [src]="data.url" style="max-width: 100%; max-height: 80vh;" />
    </mat-dialog-content>
    <mat-dialog-actions>
      <button mat-button mat-dialog-close>Close</button>
    </mat-dialog-actions>
  `,
})
export class PhotoLightboxDialogComponent {
  protected readonly data = inject<{ url: string }>(MAT_DIALOG_DATA);
}
```

## Acceptance Criteria

- [ ] Clicking a gallery image opens the lightbox dialog.
- [ ] Lightbox shows the full-size image.
- [ ] Dialog has a "Close" button.
- [ ] Lightbox does not interfere with the rest of the page when open.
