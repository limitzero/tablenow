# Task 02: Trending Section Frontend

## Status

pending

## Wave

1

## Description

Add a "Trending This Week" section to the restaurant listing page (or a dedicated home page component) that fetches `GET /api/restaurants/popular?period=week` and renders the top restaurants as horizontal-scroll cards. A period toggle (Week / Month) lets users switch views.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** None

## Files to Create

- `client/src/app/features/restaurants/components/trending-section/trending-section.component.ts`
- `client/src/app/features/restaurants/components/trending-section/trending-section.component.html`

## Files to Modify

- `client/src/app/features/restaurants/components/restaurant-list/restaurant-list.component.html` — Add `<app-trending-section>` above the main grid.
- `client/src/app/features/restaurants/index.ts` — Export `TrendingSectionComponent`.

## Technical Details

```typescript
@Component({
  selector: 'app-trending-section',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatCardModule, MatButtonToggleModule],
})
export class TrendingSectionComponent {
  protected readonly period = signal<'week' | 'month'>('week');
  protected readonly trendingResource = httpResource<PopularRestaurant[]>(
    () => `${environment.apiBaseUrl}/restaurants/popular?period=${this.period()}`
  );
}
```

```html
<section class="trending">
  <h2>Trending</h2>
  <mat-button-toggle-group [value]="period()" (change)="period.set($event.value)">
    <mat-button-toggle value="week">This Week</mat-button-toggle>
    <mat-button-toggle value="month">This Month</mat-button-toggle>
  </mat-button-toggle-group>
  <div class="trending-scroll">
    @for (r of trendingResource.value(); track r.restaurantId) {
      <mat-card class="trending-card">
        <img mat-card-image [src]="r.thumbnailUrl" loading="lazy" />
        <mat-card-header>
          <mat-card-title>{{ r.name }}</mat-card-title>
          <mat-card-subtitle>{{ r.bookingCount }} bookings</mat-card-subtitle>
        </mat-card-header>
      </mat-card>
    }
  </div>
</section>
```

## Acceptance Criteria

- [ ] Trending section renders top restaurants for the selected period.
- [ ] Week/Month toggle switches the displayed list.
- [ ] Loading state is handled gracefully.
- [ ] `OnPush` change detection.
