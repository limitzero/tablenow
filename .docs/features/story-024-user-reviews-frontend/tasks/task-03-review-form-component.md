# Task 03: Review Submission Form

## Status

pending

## Wave

2

## Description

Add a review submission form to `RestaurantDetailComponent` shown only when the user is authenticated. The form has a star rating selector (1–5) and a text area. On submission, calls `ReviewsService.submitReview` and refreshes the reviews list on success.

## Dependencies

**Depends on:** task-01-review-service-models.md
**Blocks:** None

**Context from dependencies:** task-01 defined `ReviewsService.submitReview(restaurantId, payload)`. The `AuthService.isAuthenticated` signal from STORY-008 is in `core/`.

## Files to Modify

- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.ts` — Add `ReactiveFormsModule`, form group, and `submitReview()` method.
- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.html` — Add `@if (authService.isAuthenticated())` form section.

## Technical Details

```typescript
protected readonly reviewForm = new FormGroup({
  rating: new FormControl<number>(5, { nonNullable: true, validators: [Validators.min(1), Validators.max(5)] }),
  body: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
});

submitReview(): void {
  if (this.reviewForm.invalid) return;
  this.reviewsService.submitReview(this.restaurantId, this.reviewForm.getRawValue())
    .subscribe(() => {
      this.reviewForm.reset({ rating: 5, body: '' });
      // Trigger reviews re-fetch (signal update or reload)
    });
}
```

```html
@if (authService.isAuthenticated()) {
  <form [formGroup]="reviewForm" (ngSubmit)="submitReview()">
    <mat-form-field>
      <mat-label>Rating (1-5)</mat-label>
      <input matInput type="number" formControlName="rating" min="1" max="5" />
    </mat-form-field>
    <mat-form-field>
      <textarea matInput formControlName="body" placeholder="Write your review..."></textarea>
    </mat-form-field>
    <button mat-raised-button color="primary" type="submit" [disabled]="reviewForm.invalid">
      Submit Review
    </button>
  </form>
}
```

## Acceptance Criteria

- [ ] Form is visible only for authenticated users.
- [ ] Submitting a valid form calls `ReviewsService.submitReview`.
- [ ] Form resets after successful submission.
- [ ] Unauthenticated users do not see the form.
