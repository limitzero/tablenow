# Task 01: Review List Component

## Status

pending

## Wave

1

## Description

Creates a standalone `ReviewListComponent` that accepts a `restaurantId` input and fetches reviews from `GET /api/restaurants/{id}/reviews`. Displays author name, star rating (via MatIcon or custom), body, and timestamp.

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-023 GET reviews endpoint)
**Blocks:** task-03-reviews-integration.md

**Context from dependencies:** STORY-023 task-03 created `GET /api/restaurants/{id}/reviews` returning `[{reviewId, authorName, rating, body, createdAt}]`.

## Files to Create

- `client/src/app/features/restaurants/components/review-list.component.ts`
- `client/src/app/features/restaurants/models/review.model.ts`

## Technical Details

### Code Snippets

```typescript
// models/review.model.ts
export interface ReviewDto {
  reviewId: string;
  authorName: string;
  rating: number;
  body: string;
  createdAt: string;
}
```

```typescript
// review-list.component.ts
@Component({
  selector: 'app-review-list',
  standalone: true,
  imports: [MatListModule, MatIconModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="reviews-section">
      <h3>Reviews</h3>
      @if (reviewsResource.isLoading()) {
        <mat-spinner diameter="24" />
      } @else if (reviewsResource.value()?.length === 0) {
        <p>No reviews yet.</p>
      } @else {
        @for (review of reviewsResource.value() ?? []; track review.reviewId) {
          <div class="review-item">
            <div class="review-header">
              <strong>{{ review.authorName }}</strong>
              <span class="stars">
                @for (n of stars(review.rating); track n) {
                  <mat-icon>star</mat-icon>
                }
              </span>
              <span class="date">{{ review.createdAt | date }}</span>
            </div>
            <p>{{ review.body }}</p>
          </div>
        }
      }
    </div>
  `,
})
export class ReviewListComponent {
  readonly restaurantId = input.required<string>();
  readonly refreshTrigger = input<number>(0); // increment to refresh

  readonly reviewsResource = httpResource<ReviewDto[]>(
    () => `${environment.apiBaseUrl}/restaurants/${this.restaurantId()}/reviews`
  );

  stars(rating: number): number[] {
    return Array.from({ length: rating }, (_, i) => i);
  }
}
```

## Acceptance Criteria

- [ ] Fetches reviews from `GET /api/restaurants/{id}/reviews`
- [ ] Displays author name, star rating (icons), body, date
- [ ] Loading state shown
- [ ] "No reviews yet." when empty
- [ ] OnPush, standalone
