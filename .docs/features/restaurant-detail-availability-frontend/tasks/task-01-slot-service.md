# Task 01: Slot Availability Service & Models

## Status

pending

## Wave

1

## Description

Creates the `SlotAvailabilityService` and TypeScript models for the slot availability feature. The service wraps `httpResource()` for fetching available slots from `GET /api/restaurants/{id}/slots`. Models define `TimeSlot` and `SlotQuery` interfaces.

## Dependencies

**Depends on:** STORY-002 task-03-environment-config.md
**Blocks:** task-03-detail-page.md

**Context from dependencies:** `environment.apiBaseUrl = 'http://localhost:5000/api'`. Slot API: `GET /api/restaurants/{id}/slots?date=YYYY-MM-DD&partySize=N` → `[{ slotId, time, remainingCapacity }]`.

## Files to Create

- `client/src/app/features/restaurants/models/slot.model.ts`
- `client/src/app/features/restaurants/services/slot-availability.service.ts`

## Technical Details

### Code Snippets

```typescript
// slot.model.ts
export interface TimeSlot {
  slotId: string;
  time: string; // "18:00"
  remainingCapacity: number;
}

export interface SlotQuery {
  restaurantId: string;
  date: string; // "YYYY-MM-DD"
  partySize: number;
}
```

```typescript
// slot-availability.service.ts
import { inject, Injectable, Signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { TimeSlot, SlotQuery } from '../models/slot.model';

@Injectable({ providedIn: 'root' })
export class SlotAvailabilityService {
  private readonly http = inject(HttpClient);

  getSlotsUrl(query: SlotQuery): string {
    const params = new URLSearchParams({
      date: query.date,
      partySize: query.partySize.toString(),
    });
    return `${environment.apiBaseUrl}/restaurants/${query.restaurantId}/slots?${params}`;
  }
}
```

## Acceptance Criteria

- [ ] `TimeSlot` and `SlotQuery` interfaces exist
- [ ] `SlotAvailabilityService` provides a URL builder for slot queries
- [ ] `npm run build` exits with code 0
