# Task 01: Star Rating Component

## Status

pending

## Wave

1

## Description

Creates a reusable `StarRatingComponent` that renders 5 interactive stars. Supports both read-only display mode (for review lists) and interactive input mode (for the submission form). Uses Angular signal inputs.

## Dependencies

**Depends on:** STORY-002 task-03-environment-config.md
**Blocks:** task-03-review-form.md

## Files to Create

- `client/src/app/shared/components/star-rating/star-rating.component.ts`
- `client/src/app/shared/components/star-rating/star-rating.component.html`
- `client/src/app/shared/components/star-rating/star-rating.component.scss`

## Technical Details

### Code Snippets

```typescript
@Component({
  selector: 'app-star-rating',
  standalone: true,
  template: `
    @for (star of stars; track star) {
      <button
        [class.filled]="star <= (hovered() || value())"
        [disabled]="readonly()"
        (click)="setValue(star)"
        (mouseenter)="hovered.set(star)"
        (mouseleave)="hovered.set(0)">
        ★
      </button>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StarRatingComponent {
  value = model<number>(0);
  readonly = input(false);
  stars = [1, 2, 3, 4, 5];
  hovered = signal(0);

  setValue(star: number): void {
    if (!this.readonly()) this.value.set(star);
  }
}
```

## Acceptance Criteria

- [ ] 5 stars rendered
- [ ] Clicking sets `value` signal
- [ ] Hovering previews selection
- [ ] `readonly` input disables interaction
- [ ] `changeDetection: OnPush`
- [ ] `npm run build` exits with code 0
