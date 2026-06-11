# Task 02: Restaurant Card Component

## Status

pending

## Wave

1

## Description

Creates a reusable `RestaurantCardComponent` that displays name, cuisine badge, address, and a thumbnail image. Emits a `selected` output event. Used by the listing page.

## Dependencies

**Depends on:** STORY-002 task-03-environment-config.md (Angular Material configured)
**Blocks:** task-03-listing-page.md

**Context from dependencies:** Angular Material is configured. The `Restaurant` model is created in task-01 (parallel): `{ id, name, cuisine, address, description, thumbnailUrl }`.

## Files to Create

- `client/src/app/features/restaurants/components/restaurant-card/restaurant-card.component.ts`
- `client/src/app/features/restaurants/components/restaurant-card/restaurant-card.component.html`
- `client/src/app/features/restaurants/components/restaurant-card/restaurant-card.component.scss`

## Technical Details

### Code Snippets

```typescript
// restaurant-card.component.ts
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { Restaurant } from '../../models/restaurant.model';

@Component({
  selector: 'app-restaurant-card',
  standalone: true,
  imports: [MatCardModule, MatChipsModule],
  templateUrl: './restaurant-card.component.html',
  styleUrl: './restaurant-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RestaurantCardComponent {
  restaurant = input.required<Restaurant>();
  selected = output<Restaurant>();

  onSelect(): void {
    this.selected.emit(this.restaurant());
  }
}
```

```html
<!-- restaurant-card.component.html -->
<mat-card class="restaurant-card" (click)="onSelect()" tabindex="0" role="button">
  @if (restaurant().thumbnailUrl) {
    <img mat-card-image [src]="restaurant().thumbnailUrl" [alt]="restaurant().name" loading="lazy" />
  }
  <mat-card-header>
    <mat-card-title>{{ restaurant().name }}</mat-card-title>
    <mat-card-subtitle>{{ restaurant().address }}</mat-card-subtitle>
  </mat-card-header>
  <mat-card-content>
    <mat-chip-set>
      <mat-chip>{{ restaurant().cuisine }}</mat-chip>
    </mat-chip-set>
  </mat-card-content>
</mat-card>
```

## Acceptance Criteria

- [ ] `RestaurantCardComponent` displays name, cuisine chip, address, and thumbnail
- [ ] Clicking the card emits the `selected` output event
- [ ] `@if` used for conditional thumbnail rendering
- [ ] `changeDetection: ChangeDetectionStrategy.OnPush` set
- [ ] `input.required<Restaurant>()` used (Angular 17+ signal input)
- [ ] `npm run build` exits with code 0
