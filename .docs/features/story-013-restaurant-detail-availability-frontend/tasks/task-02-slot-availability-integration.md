# Task 02: Slot Availability Integration

## Status

pending

## Wave

2

## Description

Extend `RestaurantDetailComponent` with a reactive date/party-size form and the slot availability list. When the date or party size changes, a new `httpResource` call is made to `GET /api/restaurants/{id}/slots?date=&partySize=`. The slot list renders using `@for`; an empty state message appears when no slots are available. Each slot row shows the time and remaining seats, and is tappable (navigates to booking confirmation — STORY-015 wires the full flow).

## Dependencies

**Depends on:** task-01-restaurant-detail-component.md
**Blocks:** None

**Context from dependencies:** task-01 created `RestaurantDetailComponent` with the restaurant profile section and a `<div class="slot-panel">` placeholder. This task adds the form controls and slot list into that placeholder. The `ActivatedRoute` snapshot `restaurantId` is already available in the component.

## Files to Create

- `client/src/app/features/restaurants/models/time-slot.model.ts` — `TimeSlot` interface.

## Files to Modify

- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.ts` — Add date/partySize signals and `httpResource` for slots.
- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.html` — Add form controls and slot list.
- `client/src/app/features/restaurants/index.ts` — Export `TimeSlot` model.

## Technical Details

### Implementation Steps

1. Define `TimeSlot` interface: `{ slotId: string; time: string; remainingCapacity: number }`.
2. In the component, add: `protected readonly selectedDate = signal(defaultDate())` and `protected readonly partySize = signal(2)`.
3. `defaultDate()` returns tomorrow's date as ISO string `YYYY-MM-DD`.
4. Add `httpResource` for slots: `private readonly slotsResource = httpResource<TimeSlot[]>(() => \`\${environment.apiBaseUrl}/restaurants/\${this.restaurantId}/slots?date=\${this.selectedDate()}&partySize=\${this.partySize()}\`)`.
5. Because `httpResource` re-fetches automatically when its signal dependencies change, changing `selectedDate` or `partySize` signals triggers a new request automatically.
6. Template: add `<mat-form-field>` with `<input matDatepicker>` for date, `<mat-select>` for party size (options 1–20), and the slot list.

### Code Snippets

```typescript
// time-slot.model.ts
export interface TimeSlot {
  slotId: string;
  time: string;
  remainingCapacity: number;
}
```

```html
<!-- slot-panel section in restaurant-detail.component.html -->
<section class="slot-panel">
  <h3>Check Availability</h3>
  <mat-form-field>
    <input matInput [matDatepicker]="picker" [value]="selectedDate()"
           (dateChange)="selectedDate.set($event.value)" />
    <mat-datepicker-toggle matSuffix [for]="picker" />
    <mat-datepicker #picker />
  </mat-form-field>

  <mat-select [value]="partySize()" (selectionChange)="partySize.set($event.value)">
    @for (n of [1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20]; track n) {
      <mat-option [value]="n">{{ n }} {{ n === 1 ? 'person' : 'people' }}</mat-option>
    }
  </mat-select>

  @if (slotsResource.isLoading()) {
    <mat-spinner diameter="32" />
  } @else if (!slotsResource.value()?.length) {
    <p>No availability for this date and party size.</p>
  } @else {
    @for (slot of slotsResource.value(); track slot.slotId) {
      <div class="slot-row" role="button">
        <span>{{ slot.time | date:'shortTime' }}</span>
        <span>{{ slot.remainingCapacity }} seats left</span>
      </div>
    }
  }
</section>
```

## Acceptance Criteria

- [ ] Changing the date triggers a new slots API call and updates the list.
- [ ] Changing the party size triggers a new slots API call and updates the list.
- [ ] "No availability" message is shown when the API returns an empty array.
- [ ] Slot rows display time and remaining capacity.
- [ ] Loading spinner is shown during the API call.
- [ ] No `.subscribe()` in the component.

## Notes

- `httpResource` re-runs when signals read inside its factory function change. The `selectedDate()` and `partySize()` signals serve as reactive dependencies automatically.
- The date format sent to the API should be `YYYY-MM-DD` (ISO 8601 date only, matching `DateOnly` on the backend).
