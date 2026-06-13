# Task 02: Restaurant List Component

## Status

pending

## Wave

2

## Description

Create the `RestaurantListComponent` that renders the restaurant grid. On init, it loads restaurants from the API via `RestaurantsService` and sets them in `RestaurantsStore`. It renders Material cards for each restaurant using `@for`, shows a cuisine filter `<mat-select>` that calls `store.setCuisineFilter()`, and navigates to `/restaurants/:id` when a card is clicked. Uses `OnPush` change detection and reads all display data from the store's computed signals.

## Dependencies

**Depends on:** task-01-restaurants-store.md
**Blocks:** task-03-restaurant-routes.md

**Context from dependencies:** task-01 created `RestaurantsStore` with `filteredRestaurants()` and `availableCuisines()` computed signals, `setRestaurants(list)` and `setCuisineFilter(filter)` methods, and `RestaurantsService.getAll()` returning an `httpResource<Restaurant[]>`.

## Files to Create

- `client/src/app/features/restaurants/components/restaurant-list/restaurant-list.component.ts` — Component class.
- `client/src/app/features/restaurants/components/restaurant-list/restaurant-list.component.html` — Template.

## Files to Modify

- `client/src/app/features/restaurants/index.ts` — Export `RestaurantListComponent`.

## Technical Details

### Implementation Steps

1. Create `RestaurantListComponent` as a standalone component with `ChangeDetectionStrategy.OnPush`.
2. Inject `RestaurantsStore` and `Router` using `inject()`.
3. Create an `httpResource` in the component constructor body to fetch restaurants: `const resource = httpResource<Restaurant[]>(() => \`\${environment.apiBaseUrl}/restaurants\`)`.
4. Use `effect(() => { if (resource.value()) store.setRestaurants(resource.value()!); })` to push loaded data into the store.
5. Template: `<mat-select>` bound to `store.cuisineFilter()` with options from `store.availableCuisines()`; a `@for` loop of `<mat-card>` over `store.filteredRestaurants()`.
6. Card click: `router.navigate(['/restaurants', restaurant.id])`.
7. Show loading skeleton or `<mat-spinner>` while `resource.isLoading()`.

### Code Snippets

```typescript
@Component({
  selector: 'app-restaurant-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatCardModule, MatSelectModule, MatProgressSpinnerModule, CommonModule],
  templateUrl: './restaurant-list.component.html',
})
export class RestaurantListComponent {
  protected readonly store = inject(RestaurantsStore);
  private readonly router = inject(Router);
  private readonly resource = httpResource<Restaurant[]>(
    () => `${environment.apiBaseUrl}/restaurants`
  );

  constructor() {
    effect(() => {
      const data = this.resource.value();
      if (data) this.store.setRestaurants(data);
    });
  }

  navigateToDetail(id: string): void {
    this.router.navigate(['/restaurants', id]);
  }
}
```

```html
<!-- restaurant-list.component.html -->
<div class="filter-bar">
  <mat-select placeholder="All cuisines"
              [value]="store.cuisineFilter()"
              (selectionChange)="store.setCuisineFilter($event.value)">
    <mat-option value="">All cuisines</mat-option>
    @for (cuisine of store.availableCuisines(); track cuisine) {
      <mat-option [value]="cuisine">{{ cuisine }}</mat-option>
    }
  </mat-select>
</div>

@if (resource.isLoading()) {
  <mat-spinner />
} @else {
  <div class="restaurant-grid">
    @for (restaurant of store.filteredRestaurants(); track restaurant.id) {
      <mat-card (click)="navigateToDetail(restaurant.id)" class="restaurant-card">
        <img mat-card-image [src]="restaurant.thumbnailUrl" [alt]="restaurant.name" loading="lazy" />
        <mat-card-header>
          <mat-card-title>{{ restaurant.name }}</mat-card-title>
          <mat-card-subtitle>{{ restaurant.cuisine }}</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <p>{{ restaurant.address }}</p>
        </mat-card-content>
      </mat-card>
    }
    @if (store.filteredRestaurants().length === 0) {
      <p>No restaurants match the selected cuisine.</p>
    }
  </div>
}
```

## Acceptance Criteria

- [ ] Component renders a card for each restaurant in `store.filteredRestaurants()`.
- [ ] Cuisine filter updates `store.cuisineFilter()` and the card list updates reactively.
- [ ] Clicking a card navigates to `/restaurants/:id`.
- [ ] A loading indicator is shown while the API call is in progress.
- [ ] Component uses `OnPush` change detection.
- [ ] No `.subscribe()` calls in the component.

## Notes

- `httpResource` is Angular 19+ API. If a different version is used, substitute with `toSignal(this.http.get<Restaurant[]>(...))` in a service.
