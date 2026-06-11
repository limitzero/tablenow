# Task 14: Restaurant Detail & Availability Frontend

## Status

pending

## Phase

8

## Description

Build the restaurant detail page at `/restaurants/:id`. It shows full restaurant info and a reactive availability form: a date picker (defaulting to tomorrow) and a party size selector (1–20). When either value changes, the slot list refreshes by calling `GET /api/v1/restaurants/{id}/slots?date=&partySize=`. Each slot shows its time and remaining capacity. Clicking a slot emits an event consumed by the booking confirmation flow added in task-16.

## Dependencies

**Depends on:** task-13-restaurant-listing-frontend, task-07-restaurant-slot-api  
**Blocks:** task-16-reservation-booking-frontend

**Context from dependencies:** task-13 created `restaurants.routes.ts` with a list route and a commented placeholder for the detail route. `RestaurantsService` in `features/restaurants/services/restaurants.service.ts` already has a `getRestaurantById(id)` method. `RestaurantsStore` exists with restaurant data. task-07 implemented `GET /api/v1/restaurants/{id}/slots?date=2026-06-20&partySize=4` returning `[{slotId, time, remainingCapacity}]`.

## Files to Create

- `client/src/app/features/restaurants/models/slot.model.ts`
- `client/src/app/features/restaurants/models/restaurant-detail.model.ts`
- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.ts`
- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.html`
- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.scss`

## Files to Modify

- `client/src/app/features/restaurants/services/restaurants.service.ts` — add `getAvailableSlots(id, date, partySize)` method
- `client/src/app/features/restaurants/routes/restaurants.routes.ts` — add `':id'` route for detail component
- `client/src/app/features/restaurants/index.ts` — export detail model

## Technical Details

### Implementation Steps

1. **Define slot and restaurant-detail model interfaces.**
2. **Add `getAvailableSlots` to `RestaurantsService`.**
3. **Write `RestaurantDetailComponent`** with reactive form for date + party size, and a slot list.
4. **Add the `':id'` route** to `restaurants.routes.ts`.

### Code Snippets

**Models:**
```typescript
// slot.model.ts
export interface Slot {
  slotId: string;
  time: string;        // "HH:mm"
  remainingCapacity: number;
}

// restaurant-detail.model.ts
export interface RestaurantDetail {
  id: string;
  name: string;
  cuisine: string;
  address: string;
  description: string;
  thumbnailUrl: string | null;
}
```

**`RestaurantsService` addition:**
```typescript
getAvailableSlots(restaurantId: string, date: string, partySize: number) {
  return this.http.get<Slot[]>(
    `${this.base}/restaurants/${restaurantId}/slots`,
    { params: { date, partySize: partySize.toString() } }
  );
}
```

**`restaurant-detail.component.ts`:**
```typescript
import {
  Component, ChangeDetectionStrategy, inject, signal,
  computed, OnInit
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { switchMap, filter } from 'rxjs';
import { RestaurantsService } from '../../services/restaurants.service';
import { RestaurantDetail } from '../../models/restaurant-detail.model';
import { Slot } from '../../models/slot.model';
import { MatCardModule } from '@angular/material/card';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatNativeDateModule } from '@angular/material/core';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-restaurant-detail',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatCardModule, MatDatepickerModule,
    MatInputModule, MatSelectModule, MatNativeDateModule,
    MatListModule, MatButtonModule,
  ],
  templateUrl: './restaurant-detail.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RestaurantDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly service = inject(RestaurantsService);
  private readonly fb = inject(FormBuilder);

  readonly restaurantId = this.route.snapshot.paramMap.get('id')!;
  readonly restaurant = signal<RestaurantDetail | null>(null);
  readonly slots = signal<Slot[]>([]);
  readonly slotsLoading = signal(false);
  readonly selectedSlot = signal<Slot | null>(null);

  readonly tomorrow = new Date(Date.now() + 86_400_000);
  readonly minDate = this.tomorrow;

  readonly form = this.fb.group({
    date: [this.tomorrow, Validators.required],
    partySize: [2, [Validators.required, Validators.min(1), Validators.max(20)]],
  });

  readonly partySizes = Array.from({ length: 20 }, (_, i) => i + 1);

  ngOnInit(): void {
    this.service.getRestaurantById(this.restaurantId).subscribe(r =>
      this.restaurant.set(r)
    );
    this.loadSlots();

    // Reload slots on form value changes
    this.form.valueChanges.subscribe(() => {
      if (this.form.valid) this.loadSlots();
    });
  }

  selectSlot(slot: Slot): void {
    this.selectedSlot.set(slot);
    // Task-16 listens to this signal to open the confirmation dialog
  }

  private loadSlots(): void {
    const { date, partySize } = this.form.getRawValue();
    if (!date || !partySize) return;

    const dateStr = date.toISOString().split('T')[0]; // "yyyy-MM-dd"
    this.slotsLoading.set(true);

    this.service.getAvailableSlots(this.restaurantId, dateStr, partySize).subscribe({
      next: slots => { this.slots.set(slots); this.slotsLoading.set(false); },
      error: () => { this.slots.set([]); this.slotsLoading.set(false); },
    });
  }
}
```

**`restaurant-detail.component.html`:**
```html
@if (restaurant(); as r) {
  <div class="detail-container">
    <h1>{{ r.name }}</h1>
    <p class="cuisine-badge">{{ r.cuisine }}</p>
    <p>{{ r.address }}</p>
    <p>{{ r.description }}</p>

    <section class="availability-section">
      <h2>Check Availability</h2>
      <form [formGroup]="form" class="availability-form">
        <mat-form-field appearance="outline">
          <mat-label>Date</mat-label>
          <input matInput [matDatepicker]="picker" formControlName="date" [min]="minDate" />
          <mat-datepicker-toggle matSuffix [for]="picker" />
          <mat-datepicker #picker />
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Party size</mat-label>
          <mat-select formControlName="partySize">
            @for (n of partySizes; track n) {
              <mat-option [value]="n">{{ n }} {{ n === 1 ? 'person' : 'people' }}</mat-option>
            }
          </mat-select>
        </mat-form-field>
      </form>

      @if (slotsLoading()) {
        <mat-spinner diameter="32" />
      } @else if (slots().length === 0) {
        <p class="no-slots">No availability for this date and party size.</p>
      } @else {
        <mat-list>
          @for (slot of slots(); track slot.slotId) {
            <mat-list-item>
              <span>{{ slot.time }}</span>
              <span class="capacity">{{ slot.remainingCapacity }} seats available</span>
              <button mat-stroked-button (click)="selectSlot(slot)">Book</button>
            </mat-list-item>
          }
        </mat-list>
      }
    </section>
  </div>
}
```

**`restaurants.routes.ts` update (add detail route):**
```typescript
export const RESTAURANT_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('../components/restaurant-list/restaurant-list.component')
        .then(m => m.RestaurantListComponent),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('../components/restaurant-detail/restaurant-detail.component')
        .then(m => m.RestaurantDetailComponent),
  },
];
```

## Acceptance Criteria

- [ ] `/restaurants/:id` shows the restaurant's name, cuisine, address, and description
- [ ] Date picker defaults to tomorrow; selecting a new date refreshes the slot list
- [ ] Party size selector (1–20) changing refreshes the slot list
- [ ] Slot list shows only slots with `remainingCapacity >= partySize`
- [ ] Empty slot list shows "No availability for this date and party size."
- [ ] Clicking "Book" on a slot sets `selectedSlot` signal (consumed by task-16 confirmation dialog)
- [ ] `ChangeDetectionStrategy.OnPush` used; no `*ngIf`/`*ngFor`; no constructor injection

## Notes

- The `selectedSlot` signal exposed on this component is the handoff point to task-16. task-16 will inject this component's store or use router state to receive the selected slot and open the confirmation dialog.
- `toISOString().split('T')[0]` is a safe way to format a `Date` object to `yyyy-MM-dd` without a date library.
- The `MatDatepicker` requires `MatNativeDateModule` (for basic JS `Date` support) — do not use `MatMomentDateModule` unless Moment.js is added to the project.
- Use `partySize.toString()` when building the `HttpParams` — the API expects `partySize` as a number string in the query string.
