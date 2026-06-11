# Task 03: Restaurant Detail Page Component

## Status

pending

## Wave

2

## Description

Creates the `/restaurants/:id` detail page component. Displays restaurant info, mounts `AvailabilityControlsComponent`, and uses `httpResource()` with a reactive URL (keyed on date + partySize) to fetch and display available slots. Shows "No availability" when the list is empty.

## Dependencies

**Depends on:** task-01-slot-service.md, task-02-availability-controls.md
**Blocks:** STORY-015 (booking flow is added to this page)

**Context from dependencies:**
- task-01 created `SlotAvailabilityService.getSlotsUrl(query)` and `TimeSlot { slotId, time, remainingCapacity }`
- task-02 created `AvailabilityControlsComponent` emitting `queryChanged: AvailabilityQuery` with `date` and `partySize`
- `Restaurant` model (from STORY-012 task-01): `{ id, name, cuisine, address, description, thumbnailUrl }`
- Route param: `:id` accessible via `input()` with `withComponentInputBinding()` router option

## Files to Create

- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.ts`
- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.html`
- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.scss`

## Files to Modify

- `client/src/app/features/restaurants/routes.ts` — Ensure `:id` route points to this component (may already exist as stub from STORY-012 task-03)

## Technical Details

### Code Snippets

```typescript
// restaurant-detail.component.ts
import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { AvailabilityControlsComponent, AvailabilityQuery } from '../availability-controls/availability-controls.component';
import { SlotAvailabilityService } from '../../services/slot-availability.service';
import { Restaurant } from '../../models/restaurant.model';
import { TimeSlot } from '../../models/slot.model';

@Component({
  selector: 'app-restaurant-detail',
  standalone: true,
  imports: [AvailabilityControlsComponent],
  templateUrl: './restaurant-detail.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RestaurantDetailComponent {
  readonly id = input.required<string>(); // bound from route param via withComponentInputBinding

  private readonly slotService = inject(SlotAvailabilityService);

  private readonly availabilityQuery = signal<AvailabilityQuery | null>(null);

  readonly restaurant = httpResource<Restaurant>(
    computed(() => `${environment.apiBaseUrl}/restaurants/${this.id()}`)
  );

  readonly slots = httpResource<TimeSlot[]>(
    computed(() => {
      const q = this.availabilityQuery();
      if (!q) return null;
      return this.slotService.getSlotsUrl({ restaurantId: this.id(), ...q });
    })
  );

  onQueryChanged(query: AvailabilityQuery): void {
    this.availabilityQuery.set(query);
  }
}
```

```html
<!-- restaurant-detail.component.html -->
@if (restaurant.value(); as r) {
  <div class="detail-header">
    <h1>{{ r.name }}</h1>
    <p class="cuisine-badge">{{ r.cuisine }}</p>
    <p>{{ r.address }}</p>
    <p>{{ r.description }}</p>
  </div>
}

<section class="availability">
  <h2>Check Availability</h2>
  <app-availability-controls (queryChanged)="onQueryChanged($event)" />

  @if (slots.isLoading()) {
    <p>Loading slots...</p>
  } @else if (slots.value()?.length) {
    <ul class="slot-list">
      @for (slot of slots.value()!; track slot.slotId) {
        <li>{{ slot.time }} — {{ slot.remainingCapacity }} seats remaining</li>
      }
    </ul>
  } @else {
    <p class="no-availability">No availability for this date and party size.</p>
  }
</section>
```

## Acceptance Criteria

- [ ] `/restaurants/:id` displays restaurant name, cuisine, address, description
- [ ] `AvailabilityControlsComponent` is mounted and `queryChanged` triggers slot refresh
- [ ] Slot list updates when date or party size changes
- [ ] "No availability..." message shown when slot list is empty
- [ ] Loading state shown during slot fetch
- [ ] `changeDetection: ChangeDetectionStrategy.OnPush`
- [ ] `@if` / `@for` used throughout
- [ ] `npm run build` exits with code 0
