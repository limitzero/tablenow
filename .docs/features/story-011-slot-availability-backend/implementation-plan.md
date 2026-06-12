# Implementation Plan: Slot Availability — Backend

## Overview

Adds `GET /api/restaurants/{id}/slots?date=&partySize=` to the TableNow backend. The endpoint returns only `TimeSlot` rows where `RemainingCapacity >= partySize` for the requested date, filtered entirely in the database. Three tasks across two phases: Phase 1 builds the Data and Application layers in parallel; Phase 2 wires the HTTP endpoint, mapper, and BDD unit tests.

## Phase 1: Data & Application Layers

Implement the CQRS query that filters slots in the database and the application handler that maps the result to the public API contract. Both tasks touch separate projects (`Data/Restaurants` vs `Application/Restaurants`) and can run in parallel.

### Tasks

- [x] **task-01** — `GetAvailableSlotsQuery(RestaurantId, Date, PartySize)` + `SlotData` record + `GetAvailableSlotsQueryHandler` using EF LINQ filter
- [x] **task-02** — `GetAvailableSlotsRequest` + `GetAvailableSlotsRequestHandler` dispatching to task-01's query via `IMediator`, mapping `SlotData → TimeSlotResponse`

### Technical Details

**Data layer query** (`server/src/Data/Restaurants/Queries/GetAvailableSlots/`):
- Convert `DateOnly` → UTC `DateTimeOffset` window (`startUtc`, `endUtc = startUtc.AddDays(1)`)
- EF LINQ: `.Where(ts => ts.RestaurantId == ... && ts.StartTime >= startUtc && ts.StartTime < endUtc && ts.RemainingCapacity >= query.PartySize)`
- `.OrderBy(ts => ts.StartTime).Select(ts => new SlotData(ts.Id, ts.StartTime, ts.RemainingCapacity)).ToListAsync(ct)`
- Return `Result<IReadOnlyList<SlotData>>.Success(slots)` — empty list is success

**Application layer** (`server/src/Application/Restaurants/Features/GetAvailableSlots/`):
- Dispatch `new GetAvailableSlotsQuery(...)` via `IMediator`
- Propagate failure with `Result<IReadOnlyList<TimeSlotResponse>>.Failure(dataResult.StatusCode, [.. dataResult.Errors])`
- Map: `dataResult.Data!.Select(d => new TimeSlotResponse(d.SlotId, d.Time, d.RemainingCapacity)).ToList()`

## Phase 2: API Endpoint, Mapper & Tests

Expose the HTTP endpoint, extend the static mapper, and verify the data-layer behavior with BDD unit tests. Depends on both Phase 1 tasks completing.

### Tasks

- [x] **task-03** — `GET /{id:guid}/slots` route in `RestaurantEndpoints.cs` with inline validation, `RestaurantMapper.ToGetAvailableSlotsRequest`, and BDD tests in `describe_get_available_slots.cs`

### Technical Details

**Validation** (inline in the endpoint, not FluentValidation):
```csharp
var errors = new Dictionary<string, string[]>();
if (date is null) errors["date"] = ["date is required and must be in yyyy-MM-dd format."];
if (partySize is null or <= 0) errors["partySize"] = ["partySize must be a positive integer."];
if (errors.Count > 0) return Results.ValidationProblem(errors);
```

**Mapper addition** (`server/src/Api/Mappers/RestaurantMapper.cs`):
```csharp
public static GetAvailableSlotsRequest ToGetAvailableSlotsRequest(
    Guid restaurantId, DateOnly date, int partySize) => new(restaurantId, date, partySize);
```

**BDD tests** (`server/tests/UnitTests/describe_get_available_slots.cs`):
- Use `Microsoft.Data.Sqlite` in-memory + `IAsyncLifetime`
- `when_party_size_exceeds_remaining_capacity` → `it_should_not_return_the_slot`
- `when_slot_has_sufficient_capacity` → `it_should_return_the_slot`
