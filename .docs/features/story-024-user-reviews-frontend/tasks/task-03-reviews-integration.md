# Task 03: Reviews Integration

## Status

pending

## Wave

2

## Description

Adds `ReviewListComponent` and `ReviewFormComponent` to `RestaurantDetailComponent`. The form is only shown when `isAuthenticated`. After form submission, the review list refreshes.

## Dependencies

**Depends on:** task-01-review-list-component.md, task-02-review-form-component.md
**Blocks:** Nothing

**Context from dependencies:** task-01 created `ReviewListComponent` with a `refreshTrigger` input. task-02 created `ReviewFormComponent` with `reviewSubmitted` output event. STORY-013 created `RestaurantDetailComponent`. STORY-008 task-01 created `AuthService.isAuthenticated`.

## Files to Modify

- `client/src/app/features/restaurants/components/restaurant-detail.component.ts`

## Technical Details

### Implementation Steps

```typescript
// Add to RestaurantDetailComponent imports:
import { ReviewListComponent } from './review-list.component';
import { ReviewFormComponent } from './review-form.component';
import { AuthService } from '../../../core/services/auth.service';

// New injections:
protected readonly authService = inject(AuthService);
readonly reviewRefreshTrigger = signal(0);

onReviewSubmitted(): void {
  this.reviewRefreshTrigger.update(v => v + 1);
}
```

```html
<!-- Add to template after availability section: -->
<div class="reviews-container">
  <app-review-list
    [restaurantId]="restaurantId()!"
    [refreshTrigger]="reviewRefreshTrigger()" />

  @if (authService.isAuthenticated()) {
    <app-review-form
      [restaurantId]="restaurantId()!"
      (reviewSubmitted)="onReviewSubmitted()" />
  }
</div>
```

Also add `ReviewListComponent` and `ReviewFormComponent` to the `imports` array of `RestaurantDetailComponent`.

## Acceptance Criteria

- [ ] `ReviewListComponent` added to detail page template
- [ ] `ReviewFormComponent` shown only when `authService.isAuthenticated()` is true
- [ ] On `reviewSubmitted` event: `reviewRefreshTrigger` incremented to trigger refresh
- [ ] Both components in the `imports` array of `RestaurantDetailComponent`
