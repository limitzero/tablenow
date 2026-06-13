# Task 01: Review Service & Models

## Status

pending

## Wave

1

## Description

Define the `Review` TypeScript interface and create `ReviewsService` with a method to fetch reviews via `httpResource` and a method to submit a new review via `HttpClient.post`. This is the data layer for the reviews feature.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-02-review-list-component.md, task-03-review-form-component.md

## Files to Create

- `client/src/app/features/restaurants/models/review.model.ts` — `Review` interface.
- `client/src/app/features/restaurants/services/reviews.service.ts` — `ReviewsService`.

## Files to Modify

- `client/src/app/features/restaurants/index.ts` — Export `Review` model and `ReviewsService`.

## Technical Details

```typescript
// review.model.ts
export interface Review {
  id: string;
  authorName: string;
  rating: number;
  body: string;
  createdAt: string;
}

export interface SubmitReviewPayload {
  rating: number;
  body: string;
}
```

```typescript
// reviews.service.ts
@Injectable({ providedIn: 'root' })
export class ReviewsService {
  private readonly http = inject(HttpClient);

  getReviews(restaurantId: string) {
    return httpResource<Review[]>(
      () => `${environment.apiBaseUrl}/restaurants/${restaurantId}/reviews`
    );
  }

  submitReview(restaurantId: string, payload: SubmitReviewPayload): Observable<void> {
    return this.http.post<void>(
      `${environment.apiBaseUrl}/restaurants/${restaurantId}/reviews`,
      payload
    );
  }
}
```

## Acceptance Criteria

- [ ] `Review` interface has `id`, `authorName`, `rating`, `body`, `createdAt`.
- [ ] `ReviewsService.getReviews` returns an `httpResource<Review[]>`.
- [ ] `ReviewsService.submitReview` posts to the reviews endpoint.
