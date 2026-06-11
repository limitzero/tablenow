# Task 03: Home Page "Most Booked" Section

## Status

pending

## Wave

2

## Description

Creates a home page component or adds a "Most Booked" section to the restaurant listing page. Uses `httpResource()` to fetch `GET /api/restaurants/popular?period=week`. Displays the top restaurants as cards.

## Dependencies

**Depends on:** task-01-popular-endpoint.md, task-02-cache-job.md, STORY-012 task-02-restaurant-card.md
**Blocks:** Nothing

## Files to Create

- `client/src/app/features/restaurants/components/popular-restaurants/popular-restaurants.component.ts`
- `client/src/app/features/restaurants/components/popular-restaurants/popular-restaurants.component.html`

## Files to Modify

- `client/src/app/features/restaurants/components/restaurant-list/restaurant-list.component.html` — Add popular section before grid

## Technical Details

```typescript
@Component({
  selector: 'app-popular-restaurants',
  standalone: true,
  imports: [RestaurantCardComponent],
  template: `
    @if (popular.value()?.length) {
      <section>
        <h2>Most Booked This Week</h2>
        <div class="popular-grid">
          @for (r of popular.value()!; track r.restaurantId) {
            <app-restaurant-card [restaurant]="r" (selected)="onSelected($event)" />
          }
        </div>
      </section>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PopularRestaurantsComponent {
  selected = output<Restaurant>();
  readonly popular = httpResource<Restaurant[]>(`${environment.apiBaseUrl}/restaurants/popular?period=week`);
  onSelected(r: Restaurant): void { this.selected.emit(r); }
}
```

## Acceptance Criteria

- [ ] "Most Booked" section visible on restaurant listing page
- [ ] Shows top restaurants from the `/popular` endpoint
- [ ] Clicking navigates to restaurant detail
- [ ] Section hidden when API returns empty list
- [ ] `npm run build` exits with code 0
