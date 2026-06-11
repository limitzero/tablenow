# Task 02: Review Form Component

## Status

pending

## Wave

1

## Description

Creates a standalone `ReviewFormComponent` that emits a `reviewSubmitted` event after a successful POST. Only displayed to authenticated users (caller checks `isAuthenticated`). Rating is a 1–5 integer selector.

## Dependencies

**Depends on:** None (Wave 1 — parallel with task-01, different file)
**Blocks:** task-03-reviews-integration.md

**Context from dependencies:** STORY-023 task-02 created `POST /api/restaurants/{id}/reviews` accepting `{rating, body}`.

## Files to Create

- `client/src/app/features/restaurants/components/review-form.component.ts`

## Technical Details

### Code Snippets

```typescript
// review-form.component.ts
@Component({
  selector: 'app-review-form',
  standalone: true,
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="review-form">
      <h4>Write a Review</h4>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <mat-form-field>
          <mat-label>Rating</mat-label>
          <mat-select formControlName="rating">
            @for (n of [1,2,3,4,5]; track n) {
              <mat-option [value]="n">{{ n }} star{{ n !== 1 ? 's' : '' }}</mat-option>
            }
          </mat-select>
        </mat-form-field>
        <mat-form-field>
          <mat-label>Your review</mat-label>
          <textarea matInput formControlName="body" rows="4"></textarea>
        </mat-form-field>
        @if (error()) { <p class="error">{{ error() }}</p> }
        <button mat-raised-button color="primary" type="submit" [disabled]="loading()">
          Submit Review
        </button>
      </form>
    </div>
  `,
})
export class ReviewFormComponent {
  readonly restaurantId = input.required<string>();
  readonly reviewSubmitted = output<void>();

  private readonly fb = inject(FormBuilder);
  private readonly http = inject(HttpClient);

  readonly form = this.fb.group({
    rating: [5, [Validators.required, Validators.min(1), Validators.max(5)]],
    body: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(2000)]],
  });

  readonly error = signal<string | null>(null);
  readonly loading = signal(false);

  submit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.http.post(`${environment.apiBaseUrl}/restaurants/${this.restaurantId()}/reviews`, this.form.value)
      .subscribe({
        next: () => {
          this.loading.set(false);
          this.form.reset({ rating: 5, body: '' });
          this.reviewSubmitted.emit();
        },
        error: () => {
          this.loading.set(false);
          this.error.set('Failed to submit review. Please try again.');
        },
      });
  }
}
```

## Acceptance Criteria

- [ ] Rating selector (1–5) and body textarea
- [ ] On success: form reset + `reviewSubmitted` event emitted
- [ ] On error: inline error message shown
- [ ] OnPush, standalone, inject()
