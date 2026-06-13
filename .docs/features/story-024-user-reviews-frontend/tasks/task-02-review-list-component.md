# Task 02: Review List Component

## Status

pending

## Wave

2

## Description

Extend `RestaurantDetailComponent` with a reviews section that fetches and displays existing reviews. Each review shows the author name, star rating (1–5 filled stars), body text, and formatted timestamp. Uses `httpResource` from `ReviewsService`.

## Dependencies

**Depends on:** task-01-review-service-models.md
**Blocks:** None

**Context from dependencies:** task-01 defined `Review` model and `ReviewsService.getReviews(restaurantId)` returning an `httpResource<Review[]>`.

## Files to Modify

- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.ts` — Inject `ReviewsService`, create reviews resource.
- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.html` — Add reviews section.

## Technical Details

```html
<!-- Reviews section -->
<section class="reviews">
  <h3>Reviews</h3>
  @if (reviewsResource.isLoading()) { <mat-spinner diameter="24" /> }
  @for (review of reviewsResource.value(); track review.id) {
    <div class="review-card">
      <strong>{{ review.authorName }}</strong>
      <span>{{ review.rating }}/5</span>
      <p>{{ review.body }}</p>
      <small>{{ review.createdAt | date:'mediumDate' }}</small>
    </div>
  }
  @if (!reviewsResource.isLoading() && !reviewsResource.value()?.length) {
    <p>No reviews yet. Be the first!</p>
  }
</section>
```

## Acceptance Criteria

- [ ] Reviews section shows each review with author, rating, body, and date.
- [ ] Loading spinner visible during fetch.
- [ ] "No reviews yet" shown when list is empty.
- [ ] Uses `OnPush` change detection (inherited from detail component).
