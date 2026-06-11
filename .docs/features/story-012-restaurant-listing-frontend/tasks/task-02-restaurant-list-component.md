# Task 02: Restaurant List Component

## Status

pending

## Wave

2

## Description

Creates the `RestaurantListComponent` displaying a Material card grid with cuisine filter and navigation. Reads from `RestaurantsStore`. OnPush.

## Dependencies

**Depends on:** task-01-restaurants-store-service.md
**Blocks:** STORY-013 (detail page is navigated to from here)

**Context from dependencies:** task-01 created `RestaurantsStore` with `filteredRestaurants()` computed, `availableCuisines()`, `loadRestaurants()`, and `setCuisineFilter()`. This component injects the store and renders the data. Does not modify store or service files (parallel-safe with task-03).

## Files to Create

- `client/src/app/features/restaurants/components/restaurant-list.component.ts`

## Technical Details

### Code Snippets

```typescript
// features/restaurants/components/restaurant-list.component.ts
import { Component, ChangeDetectionStrategy, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RestaurantsStore } from '../store/restaurants.store';

@Component({
  selector: 'app-restaurant-list',
  standalone: true,
  imports: [MatCardModule, MatSelectModule, MatProgressSpinnerModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-container">
      <h1>Find a Table</h1>
      <mat-form-field>
        <mat-label>Cuisine</mat-label>
        <mat-select [value]="store.cuisineFilter()"
                    (selectionChange)="store.setCuisineFilter($event.value)">
          <mat-option [value]="null">All Cuisines</mat-option>
          @for (cuisine of store.availableCuisines(); track cuisine) {
            <mat-option [value]="cuisine">{{ cuisine }}</mat-option>
          }
        </mat-select>
      </mat-form-field>

      @if (store.loading()) {
        <mat-spinner />
      } @else {
        <div class="restaurant-grid">
          @for (restaurant of store.filteredRestaurants(); track restaurant.id) {
            <mat-card (click)="navigate(restaurant.id)" class="restaurant-card">
              <mat-card-header>
                <mat-card-title>{{ restaurant.name }}</mat-card-title>
                <mat-card-subtitle>{{ restaurant.cuisine }}</mat-card-subtitle>
              </mat-card-header>
              <mat-card-content>
                <p>{{ restaurant.address }}</p>
              </mat-card-content>
            </mat-card>
          }
        </div>
      }
    </div>
  `,
})
export class RestaurantListComponent implements OnInit {
  protected readonly store = inject(RestaurantsStore);
  private readonly router = inject(Router);

  ngOnInit() {
    this.store.loadRestaurants();
  }

  navigate(id: string) {
    this.router.navigate(['/restaurants', id]);
  }
}
```

## Acceptance Criteria

- [ ] Component renders `filteredRestaurants()` as Material cards
- [ ] Each card shows name, cuisine, address
- [ ] Cuisine `mat-select` calls `store.setCuisineFilter()`
- [ ] Clicking a card navigates to `/restaurants/:id`
- [ ] Loading spinner shown when `store.loading()` is true
- [ ] OnPush, standalone, `inject()` DI
