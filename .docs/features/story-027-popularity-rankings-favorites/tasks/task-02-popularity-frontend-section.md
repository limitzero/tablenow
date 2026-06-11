# Task 02: Popularity Frontend Section

## Status

pending

## Wave

1

## Description

Creates `PopularRestaurantsComponent` showing a "Most Booked" section with a week/month toggle. Added to the top of the restaurant listing page.

## Dependencies

**Depends on:** None (Wave 1 — parallel with task-01)
**Blocks:** Nothing

## Files to Create

- `client/src/app/features/restaurants/components/popular-restaurants.component.ts`

## Files to Modify

- `client/src/app/features/restaurants/components/restaurant-list.component.ts` — add popular section at top

## Technical Details

### Code Snippets

```typescript
// popular-restaurants.component.ts
@Component({
  selector: 'app-popular-restaurants',
  standalone: true,
  imports: [MatButtonToggleModule, MatCardModule, MatProgressSpinnerModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="popular-section">
      <div class="popular-header">
        <h2>Most Booked</h2>
        <mat-button-toggle-group [value]="period()" (change)="period.set($event.value)">
          <mat-button-toggle value="week">This Week</mat-button-toggle>
          <mat-button-toggle value="month">This Month</mat-button-toggle>
        </mat-button-toggle-group>
      </div>
      @if (popularResource.isLoading()) {
        <mat-spinner diameter="32" />
      } @else {
        <div class="popular-grid">
          @for (r of popularResource.value() ?? []; track r.id) {
            <mat-card class="popular-card" (click)="navigate(r.id)">
              <img [src]="r.thumbnailUrl" loading="lazy" alt="{{ r.name }}" />
              <mat-card-content>
                <strong>{{ r.name }}</strong>
                <p>{{ r.bookingCount }} bookings</p>
              </mat-card-content>
            </mat-card>
          }
        </div>
      }
    </div>
  `,
})
export class PopularRestaurantsComponent {
  private readonly router = inject(Router);
  readonly period = signal<'week' | 'month'>('week');

  readonly popularResource = httpResource<PopularRestaurantDto[]>(
    () => `${environment.apiBaseUrl}/restaurants/popular?period=${this.period()}`
  );

  navigate(id: string) { this.router.navigate(['/restaurants', id]); }
}

interface PopularRestaurantDto { id: string; name: string; thumbnailUrl: string; bookingCount: number; }
```

## Acceptance Criteria

- [ ] "Most Booked" section at top of restaurant listing page
- [ ] Week/month toggle changes query parameter and re-fetches
- [ ] Clicking a popular restaurant card navigates to `/restaurants/:id`
- [ ] Loading state shown
- [ ] OnPush, standalone
