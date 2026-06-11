# Task 02: Date & Party Size Selector Component

## Status

pending

## Wave

1

## Description

Creates an `AvailabilityControlsComponent` with a date picker (defaulting to tomorrow) and a party size selector (1–20). Emits a `queryChanged` output event whenever either control changes. Uses Angular Material datepicker and select.

## Dependencies

**Depends on:** STORY-002 task-03-environment-config.md (Angular Material)
**Blocks:** task-03-detail-page.md

**Context from dependencies:** Angular Material is configured. The component emits `SlotQuery` values — the `SlotQuery` interface is defined in task-01 (parallel): `{ restaurantId, date, partySize }`.

## Files to Create

- `client/src/app/features/restaurants/components/availability-controls/availability-controls.component.ts`
- `client/src/app/features/restaurants/components/availability-controls/availability-controls.component.html`

## Technical Details

### Code Snippets

```typescript
// availability-controls.component.ts
import { ChangeDetectionStrategy, Component, OnInit, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';

export interface AvailabilityQuery {
  date: string; // YYYY-MM-DD
  partySize: number;
}

@Component({
  selector: 'app-availability-controls',
  standalone: true,
  imports: [ReactiveFormsModule, MatDatepickerModule, MatNativeDateModule, MatSelectModule, MatFormFieldModule],
  templateUrl: './availability-controls.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AvailabilityControlsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);

  queryChanged = output<AvailabilityQuery>();

  readonly partySizes = Array.from({ length: 20 }, (_, i) => i + 1);
  readonly tomorrow = new Date(Date.now() + 86400000);

  form = this.fb.group({
    date: [this.tomorrow, Validators.required],
    partySize: [2, [Validators.required, Validators.min(1)]],
  });

  ngOnInit(): void {
    this.form.valueChanges.subscribe(() => {
      if (this.form.valid) {
        const { date, partySize } = this.form.getRawValue();
        this.queryChanged.emit({
          date: this.formatDate(date!),
          partySize: partySize!,
        });
      }
    });
    // Emit initial value
    this.queryChanged.emit({
      date: this.formatDate(this.tomorrow),
      partySize: 2,
    });
  }

  private formatDate(d: Date): string {
    return d.toISOString().split('T')[0];
  }
}
```

```html
<!-- availability-controls.component.html -->
<form [formGroup]="form" class="availability-controls">
  <mat-form-field appearance="outline">
    <mat-label>Date</mat-label>
    <input matInput [matDatepicker]="picker" formControlName="date" />
    <mat-datepicker-toggle matIconSuffix [for]="picker" />
    <mat-datepicker #picker />
  </mat-form-field>

  <mat-form-field appearance="outline">
    <mat-label>Party Size</mat-label>
    <mat-select formControlName="partySize">
      @for (size of partySizes; track size) {
        <mat-option [value]="size">{{ size }} {{ size === 1 ? 'person' : 'people' }}</mat-option>
      }
    </mat-select>
  </mat-form-field>
</form>
```

## Acceptance Criteria

- [ ] Date picker defaults to tomorrow's date
- [ ] Party size selector shows 1–20
- [ ] `queryChanged` emits on any form change
- [ ] Initial value emitted on `ngOnInit`
- [ ] `changeDetection: ChangeDetectionStrategy.OnPush`
- [ ] `@for` used for party size options
- [ ] `npm run build` exits with code 0
