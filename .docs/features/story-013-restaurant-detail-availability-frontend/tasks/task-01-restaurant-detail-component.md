# Task 01: Restaurant Detail Component

## Status

pending

## Wave

1

## Description

Create the `RestaurantDetailComponent` that displays the full restaurant profile for a given `:id` route parameter. Reads the restaurant from `RestaurantsStore` if already loaded (navigation from the list page), or fetches it directly via `GET /api/restaurants/{id}`. Renders name, description, cuisine, and address. Adds the `:id` route to `restaurantRoutes`. Sets up the layout placeholder for the slot availability panel (task-02).

## Dependencies

**Depends on:** None (Wave 1) — the `RestaurantsStore` and `restaurant.routes.ts` from STORY-012 tasks are available.
**Blocks:** task-02-slot-availability-integration.md

**Context from dependencies:** STORY-012 task-01 created `RestaurantsStore` with `restaurants()` signal and the `Restaurant` model (`id`, `name`, `cuisine`, `address`, `description`, `thumbnailUrl`). STORY-012 task-03 created `restaurant.routes.ts` — this task adds the `:id` route there.

## Files to Create

- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.ts` — Component class.
- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.html` — Template.

## Files to Modify

- `client/src/app/features/restaurants/routes/restaurant.routes.ts` — Add `{ path: ':id', component: RestaurantDetailComponent }`.
- `client/src/app/features/restaurants/index.ts` — Export `RestaurantDetailComponent`.

## Technical Details

### Implementation Steps

1. Create `RestaurantDetailComponent` as standalone with `OnPush`.
2. Inject `ActivatedRoute`, `RestaurantsStore`, and `HttpClient` (for fallback fetch).
3. Read `restaurantId = inject(ActivatedRoute).snapshot.params['id']` as a string.
4. Look up the restaurant: `computed(() => store.restaurants().find(r => r.id === this.restaurantId))`. If null (store not yet populated), use `httpResource<Restaurant>(() => \`\${environment.apiBaseUrl}/restaurants/\${restaurantId}\`)`.
5. Template: render name, description, cuisine, address using `@if (restaurant())`.
6. Add a `<div class="slot-panel">` placeholder (filled by task-02).
7. Add `:id` route in `restaurant.routes.ts`.

### Code Snippets

```typescript
@Component({
  selector: 'app-restaurant-detail',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatCardModule, AsyncPipe],
  templateUrl: './restaurant-detail.component.html',
})
export class RestaurantDetailComponent {
  private readonly route = inject(ActivatedRoute);
  protected readonly store = inject(RestaurantsStore);
  protected readonly restaurantId = this.route.snapshot.params['id'] as string;
  private readonly resource = httpResource<Restaurant>(
    () => `${environment.apiBaseUrl}/restaurants/${this.restaurantId}`
  );
  protected readonly restaurant = computed(
    () => this.store.restaurants().find(r => r.id === this.restaurantId)
       ?? this.resource.value()
  );
}
```

## Acceptance Criteria

- [ ] `/restaurants/:id` renders the restaurant's name, description, cuisine, and address.
- [ ] Component reads from the store when the restaurant is already loaded.
- [ ] Falls back to an API fetch when navigating directly to the URL.
- [ ] Uses `OnPush` change detection.
- [ ] `:id` route is registered in `restaurant.routes.ts`.
