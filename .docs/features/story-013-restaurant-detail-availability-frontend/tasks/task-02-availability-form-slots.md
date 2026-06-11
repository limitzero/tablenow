# Task 02: Availability Form & Slots

## Status

pending

## Wave

2

## Description

Extends `RestaurantDetailComponent` with a reactive form (date + partySize) and a slot list that re-fetches automatically on every form change. Clicking a slot stores the selected slot for use by the booking confirmation dialog (STORY-015).

## Dependencies

**Depends on:** task-01-detail-component-scaffold.md
**Blocks:** STORY-015 (booking confirmation flow needs the slot selection)

**Context from dependencies:** task-01 created `RestaurantDetailComponent` with restaurant info display. This task modifies that same component file to add the availability form and slot list. The slots endpoint `GET /api/restaurants/{id}/slots?date=&partySize=` was created in STORY-011.

## Files to Modify

- `client/src/app/features/restaurants/components/restaurant-detail.component.ts`

## Files to Create

- `client/src/app/features/restaurants/models/slot.model.ts`

## Technical Details

### Code Snippets

```typescript
// models/slot.model.ts
export interface SlotDto {
  slotId: string;
  time: string; // "HH:mm:ss" from backend TimeOnly
  remainingCapacity: number;
}
```

```typescript
// Add to RestaurantDetailComponent:

// Imports to add:
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { signal, computed } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { HttpClient } from '@angular/common/http';
import { SlotDto } from '../models/slot.model';

// Inside class:
private readonly fb = inject(FormBuilder);
private readonly http = inject(HttpClient);

readonly selectedSlot = signal<SlotDto | null>(null);

readonly availabilityForm = this.fb.group({
  date: [this.tomorrowDate(), Validators.required],
  partySize: [2, [Validators.required, Validators.min(1), Validators.max(20)]],
});

private readonly formValues = toSignal(this.availabilityForm.valueChanges, {
  initialValue: this.availabilityForm.value,
});

readonly slotsResource = httpResource<SlotDto[]>(
  () => {
    const id = this.restaurantId();
    const { date, partySize } = this.formValues() ?? {};
    if (!id || !date || !partySize) return undefined;
    return `${environment.apiBaseUrl}/restaurants/${id}/slots?date=${date}&partySize=${partySize}`;
  }
);

private tomorrowDate(): string {
  const d = new Date();
  d.setDate(d.getDate() + 1);
  return d.toISOString().split('T')[0]; // YYYY-MM-DD
}

selectSlot(slot: SlotDto) {
  this.selectedSlot.set(slot);
  // STORY-015 opens the confirmation dialog here
}
```

```html
<!-- Add to template after restaurant info: -->
<div class="availability-section">
  <h2>Reserve a Table</h2>
  <form [formGroup]="availabilityForm">
    <mat-form-field>
      <mat-label>Date</mat-label>
      <input matInput type="date" formControlName="date" />
    </mat-form-field>
    <mat-form-field>
      <mat-label>Party Size</mat-label>
      <mat-select formControlName="partySize">
        @for (n of [1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20]; track n) {
          <mat-option [value]="n">{{ n }}</mat-option>
        }
      </mat-select>
    </mat-form-field>
  </form>

  @if (slotsResource.isLoading()) {
    <mat-spinner diameter="32" />
  } @else if (slotsResource.value()?.length === 0) {
    <p class="no-availability">No availability for this date and party size.</p>
  } @else {
    <div class="slot-list">
      @for (slot of slotsResource.value() ?? []; track slot.slotId) {
        <button mat-stroked-button (click)="selectSlot(slot)">
          {{ slot.time | slice:0:5 }} — {{ slot.remainingCapacity }} seats
        </button>
      }
    </div>
  }
</div>
```

## Acceptance Criteria

- [ ] Date field defaults to tomorrow's date
- [ ] Party size defaults to 2, valid range 1–20
- [ ] Changing date or partySize triggers slot re-fetch (httpResource keyed on form values)
- [ ] Slot list renders time and remainingCapacity for each slot
- [ ] Empty slot list shows "No availability for this date and party size."
- [ ] `selectedSlot` signal updated when a slot is clicked (used by STORY-015)
