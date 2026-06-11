# Task 03: Restaurant Listing Page

## Status

pending

## Wave

2

## Description

Creates the restaurant listing page component at `/restaurants`. Uses `httpResource()` to fetch the restaurant list and `RestaurantsStore` for filtering. Renders `RestaurantCardComponent` cards in a grid. A select control filters by cuisine. Clicking a card navigates to `/restaurants/:id`.

## Dependencies

**Depends on:** task-01-restaurants-store.md, task-02-restaurant-card.md
**Blocks:** STORY-013 (detail page is navigated from here)

**Context from dependencies:**
- task-01 created `RestaurantsStore` with `filteredRestaurants` computed signal and `availableCuisines`
- task-02 created `RestaurantCardComponent` with `restaurant` signal input and `selected` output
- `environment.apiBaseUrl` = `http://localhost:5000/api`
- Route: `/restaurants` → `RESTAURANT_ROUTES` in `features/restaurants/routes.ts`

## Files to Create

- `client/src/app/features/restaurants/components/restaurant-list/restaurant-list.component.ts`
- `client/src/app/features/restaurants/components/restaurant-list/restaurant-list.component.html`
- `client/src/app/features/restaurants/components/restaurant-list/restaurant-list.component.scss`

## Files to Modify

- `client/src/app/features/restaurants/routes.ts` — Replace empty array with real routes
- `client/src/app/features/restaurants/index.ts` — Export feature routes and store

## Technical Details

### Code Snippets

```typescript
// restaurant-list.component.ts
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { httpResource } from '@angular/common/http';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { RestaurantsStore } from '../../store/restaurants.store';
import { RestaurantCardComponent } from '../restaurant-card/restaurant-card.component';
import { Restaurant } from '../../models/restaurant.model';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-restaurant-list',
  standalone: true,
  imports: [RestaurantCardComponent, MatSelectModule, MatFormFieldModule],
  templateUrl: './restaurant-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RestaurantListComponent {
  private readonly router = inject(Router);
  protected readonly store = inject(RestaurantsStore);

  private readonly restaurantsResource = httpResource<Restaurant[]>(
    `${environment.apiBaseUrl}/restaurants`
  );

  // Sync httpResource data into store when loaded
  // (implement in ngOnInit or effect)

  onCuisineFilter(cuisine: string | null): void {
    // Update store cuisineFilter
  }

  onRestaurantSelected(restaurant: Restaurant): void {
    this.router.navigate(['/restaurants', restaurant.id]);
  }
}
```

```html
<!-- restaurant-list.component.html -->
<div class="listing-header">
  <h1>Restaurants</h1>
  <mat-form-field appearance="outline">
    <mat-label>Filter by Cuisine</mat-label>
    <mat-select (selectionChange)="onCuisineFilter($event.value)">
      <mat-option [value]="null">All Cuisines</mat-option>
      @for (cuisine of store.availableCuisines(); track cuisine) {
        <mat-option [value]="cuisine">{{ cuisine }}</mat-option>
      }
    </mat-select>
  </mat-form-field>
</div>

<div class="restaurant-grid">
  @for (restaurant of store.filteredRestaurants(); track restaurant.id) {
    <app-restaurant-card
      [restaurant]="restaurant"
      (selected)="onRestaurantSelected($event)" />
  }
  @empty {
    <p>No restaurants found for this cuisine.</p>
  }
</div>
```

Update `routes.ts`:
```typescript
export const RESTAURANT_ROUTES: Routes = [
  { path: '', component: RestaurantListComponent },
  { path: ':id', loadComponent: () => import('../components/restaurant-detail/restaurant-detail.component').then(m => m.RestaurantDetailComponent) },
];
```

## Acceptance Criteria

- [ ] `/restaurants` shows a grid of restaurant cards
- [ ] Cuisine filter changes `store.cuisineFilter` and updates the displayed cards
- [ ] Clicking a card navigates to `/restaurants/:id`
- [ ] `@for` with `@empty` used for the card grid
- [ ] `npm run build` exits with code 0
