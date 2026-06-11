# Task 03: Review Submission Form & Detail Page Integration

## Status

pending

## Wave

2

## Description

Creates a `ReviewFormComponent` with star rating + text area. Conditionally shown only for authenticated users. Submitted review is prepended to the list immediately. Integrates `ReviewListComponent` and `ReviewFormComponent` into the restaurant detail page.

## Dependencies

**Depends on:** task-01-star-rating.md, task-02-review-list.md
**Blocks:** Nothing

**Context from dependencies:**
- task-01: `StarRatingComponent` with `model<number>` value signal
- task-02: `ReviewListComponent` with `reviews` input, `Review` model interface
- `isAuthenticated()` from `AuthService` (STORY-008)
- API: `POST /api/restaurants/{id}/reviews` body `{ rating, body }`; `GET /api/restaurants/{id}/reviews` returns `Review[]`

## Files to Create

- `client/src/app/features/restaurants/components/review-form/review-form.component.ts`
- `client/src/app/features/restaurants/components/review-form/review-form.component.html`

## Files to Modify

- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.ts` — Add reviews section
- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.html` — Mount review components

## Technical Details

```typescript
// review-form.component.ts
@Component({
  selector: 'app-review-form',
  standalone: true,
  imports: [StarRatingComponent, ReactiveFormsModule, MatButtonModule, MatFormFieldModule, MatInputModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReviewFormComponent {
  restaurantId = input.required<string>();
  reviewSubmitted = output<Review>();

  private readonly http = inject(HttpClient);
  form = inject(FormBuilder).group({ rating: [0, Validators.min(1)], body: ['', Validators.required] });

  submit(): void {
    if (this.form.invalid) return;
    const { rating, body } = this.form.getRawValue();
    this.http.post<Review>(
      `${environment.apiBaseUrl}/restaurants/${this.restaurantId()}/reviews`,
      { rating, body })
      .subscribe(review => {
        this.reviewSubmitted.emit(review);
        this.form.reset({ rating: 0, body: '' });
      });
  }
}
```

In detail page HTML:
```html
<section class="reviews">
  <h2>Reviews</h2>
  <app-review-list [reviews]="reviewsList()" />
  @if (authService.isAuthenticated()) {
    <app-review-form [restaurantId]="id()" (reviewSubmitted)="onReviewSubmitted($event)" />
  }
</section>
```

## Acceptance Criteria

- [ ] Review form visible only to authenticated users
- [ ] Submitted review prepended to list immediately
- [ ] Rating must be ≥ 1 (form validation)
- [ ] `npm run build` exits with code 0
