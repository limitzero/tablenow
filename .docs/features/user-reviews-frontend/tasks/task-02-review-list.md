# Task 02: Review List Component

## Status

pending

## Wave

1

## Description

Creates a `ReviewListComponent` that renders a list of reviews — each showing author name (from userId if name not available), star rating, body text, and formatted timestamp.

## Dependencies

**Depends on:** task-01-star-rating.md (runs in parallel — different files)
**Blocks:** task-03-review-form.md

**Context from dependencies:** task-01 (parallel) created `StarRatingComponent` with `value` signal input and `readonly` input. `ReviewDto` from the API: `{ reviewId, userId, rating, body, createdAt }`.

## Files to Create

- `client/src/app/features/restaurants/components/review-list/review-list.component.ts`
- `client/src/app/features/restaurants/components/review-list/review-list.component.html`
- `client/src/app/features/restaurants/models/review.model.ts`

## Technical Details

### Code Snippets

```typescript
// review.model.ts
export interface Review {
  reviewId: string;
  userId: string;
  rating: number;
  body: string;
  createdAt: string;
}
```

```typescript
@Component({
  selector: 'app-review-list',
  standalone: true,
  imports: [StarRatingComponent],
  template: `
    @for (review of reviews(); track review.reviewId) {
      <div class="review-item">
        <app-star-rating [value]="review.rating" [readonly]="true" />
        <p>{{ review.body }}</p>
        <small>{{ review.createdAt | date:'mediumDate' }}</small>
      </div>
    }
    @empty {
      <p>No reviews yet.</p>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReviewListComponent {
  reviews = input.required<Review[]>();
}
```

## Acceptance Criteria

- [ ] `ReviewListComponent` renders each review with stars, body, date
- [ ] "No reviews yet" shown for empty list
- [ ] `changeDetection: OnPush`
- [ ] `npm run build` exits with code 0
