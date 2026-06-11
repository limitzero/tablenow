# Task 01: Detail Component Scaffold

## Status

pending

## Wave

1

## Description

Creates `RestaurantDetailComponent` at `features/restaurants/components/restaurant-detail.component.ts`. Reads `:id` from route params, fetches the restaurant with `httpResource()`, and displays the full info. Serves as the base that task-02 extends with the availability form.

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-012 store/service files for `RestaurantsService`)
**Blocks:** task-02-availability-form-slots.md

**Context from dependencies:** STORY-012 task-01 created `RestaurantsService.getById(id)`. STORY-012 task-03 created the route `/restaurants/:id` that loads this component. The `RestaurantDto` interface is in `features/restaurants/models/restaurant.model.ts`. This task creates the component file that STORY-012 task-03's route references.

## Files to Create

- `client/src/app/features/restaurants/components/restaurant-detail.component.ts`

## Technical Details

### Code Snippets

```typescript
// features/restaurants/components/restaurant-detail.component.ts
import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { switchMap, map } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RestaurantsService } from '../services/restaurants.service';
import { httpResource } from '@angular/core'; // Angular 19+

@Component({
  selector: 'app-restaurant-detail',
  standalone: true,
  imports: [MatCardModule, MatProgressSpinnerModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (restaurantResource.isLoading()) {
      <mat-spinner />
    } @else if (restaurantResource.value(); as restaurant) {
      <div class="detail-container">
        <h1>{{ restaurant.name }}</h1>
        <p class="cuisine">{{ restaurant.cuisine }}</p>
        <p class="address">{{ restaurant.address }}</p>
        <p class="description">{{ restaurant.description }}</p>

        <!-- Availability form added in STORY-013 task-02 -->
      </div>
    }
  `,
})
export class RestaurantDetailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly restaurantsService = inject(RestaurantsService);

  readonly restaurantId = toSignal(
    this.route.paramMap.pipe(map(p => p.get('id') ?? ''))
  );

  readonly restaurantResource = httpResource(
    () => `${environment.apiBaseUrl}/restaurants/${this.restaurantId()}`
  );
}
```

**Note:** If `httpResource` is not yet available in the project's Angular version, use a computed observable approach with `toSignal(this.restaurantsService.getById(this.restaurantId()!))` instead.

## Acceptance Criteria

- [ ] Component exists at `features/restaurants/components/restaurant-detail.component.ts`
- [ ] Reads `id` from ActivatedRoute params
- [ ] Fetches restaurant via `RestaurantsService.getById` or `httpResource`
- [ ] Displays restaurant name, cuisine, address, description
- [ ] Loading state shown while fetching
- [ ] OnPush, standalone, `inject()` DI
