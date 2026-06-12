# Task 02: Slot Availability Application Layer

## Status

complete

## Wave

1

## Description

Implement the Application-layer CQRS request/handler that translates the slot availability use case. `GetAvailableSlotsRequest` carries `restaurantId`, `date`, and `partySize` from the API layer. `GetAvailableSlotsRequestHandler` dispatches into the Data layer via `IMediator`, maps the resulting `SlotData` list to `IReadOnlyList<TimeSlotResponse>` (the public API contract), and returns `Result<IReadOnlyList<TimeSlotResponse>>`. This layer has no EF Core dependency — it calls only the Data query via Mediator.

## Dependencies

**Depends on:** None (Wave 1 — parallel with task-01)
**Blocks:** task-03-slot-availability-endpoint.md

**Context from dependencies:** This task is Wave 1, parallel to task-01. The `SlotData` contract from task-01 must be agreed upfront: `record SlotData(Guid SlotId, DateTimeOffset Time, int RemainingCapacity)` in `CM.TableNow.Restaurants.Data.Queries.GetAvailableSlots`. The `TimeSlotResponse` contract already exists in `CM.TableNow.Contracts`: `record TimeSlotResponse(Guid SlotId, DateTimeOffset Time, int RemainingCapacity)`. STORY-001 created the `CM.TableNow.Restaurants.Application` project with Mediator configured.

## Files to Create

- `server/src/Application/Restaurants/Features/GetAvailableSlots/GetAvailableSlotsRequest.cs` — `GetAvailableSlotsRequest` Mediator request.
- `server/src/Application/Restaurants/Features/GetAvailableSlots/GetAvailableSlotsRequestHandler.cs` — Handler that dispatches to the Data query and maps the result.

## Files to Modify

- None.

## Technical Details

### Implementation Steps

1. Define `GetAvailableSlotsRequest(Guid RestaurantId, DateOnly Date, int PartySize)` as a `record` implementing the Mediator request interface returning `Result<IReadOnlyList<TimeSlotResponse>>`.
2. Implement `GetAvailableSlotsRequestHandler` using a primary constructor for `IMediator` DI.
3. In the handler, dispatch `new GetAvailableSlotsQuery(request.RestaurantId, request.Date, request.PartySize)` via `mediator.Send(...)`.
4. If `dataResult.IsSuccess` is false, propagate the failure: `Result<IReadOnlyList<TimeSlotResponse>>.Failure(dataResult.StatusCode, [.. dataResult.Errors])`.
5. Map each `SlotData` to `TimeSlotResponse(d.SlotId, d.Time, d.RemainingCapacity)` and return success.

### Code Snippets

```csharp
using CM.TableNow.Contracts;
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Restaurants.Application.Features.GetAvailableSlots;

public sealed record GetAvailableSlotsRequest(
    Guid RestaurantId,
    DateOnly Date,
    int PartySize) : IRequest<Result<IReadOnlyList<TimeSlotResponse>>>;
```

```csharp
using CM.TableNow.Contracts;
using CM.TableNow.Restaurants.Data.Queries.GetAvailableSlots;
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Restaurants.Application.Features.GetAvailableSlots;

public sealed class GetAvailableSlotsRequestHandler(IMediator mediator)
    : IRequestHandler<GetAvailableSlotsRequest, Result<IReadOnlyList<TimeSlotResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<TimeSlotResponse>>> Handle(
        GetAvailableSlotsRequest request,
        CancellationToken cancellationToken)
    {
        var dataResult = await mediator.Send(
            new GetAvailableSlotsQuery(request.RestaurantId, request.Date, request.PartySize),
            cancellationToken);

        if (!dataResult.IsSuccess)
            return Result<IReadOnlyList<TimeSlotResponse>>.Failure(dataResult.StatusCode, [.. dataResult.Errors]);

        IReadOnlyList<TimeSlotResponse> responses = dataResult.Data!
            .Select(d => new TimeSlotResponse(d.SlotId, d.Time, d.RemainingCapacity))
            .ToList();

        return Result<IReadOnlyList<TimeSlotResponse>>.Success(responses);
    }
}
```

### API Endpoints

- This layer does not expose endpoints directly. The API contract it produces:
  - Success: `Result.Success` wrapping `IReadOnlyList<TimeSlotResponse>`
  - `TimeSlotResponse`: `{ slotId: Guid, time: DateTimeOffset, remainingCapacity: int }`

## Acceptance Criteria

- [ ] `GetAvailableSlotsRequest` carries `RestaurantId`, `Date`, and `PartySize`.
- [ ] `GetAvailableSlotsRequestHandler` dispatches to `GetAvailableSlotsQuery` via `IMediator`.
- [ ] On data failure, the result propagates the status code and errors without throwing.
- [ ] On success, each `SlotData` is mapped 1:1 to a `TimeSlotResponse`.
- [ ] No EF Core or `DbContext` dependency in this project.
- [ ] Handler accepts and propagates a `CancellationToken`.

## Notes

- No static mapper is needed here — the `SlotData → TimeSlotResponse` projection is trivial enough to inline in the handler. The static `RestaurantMapper` in the Api project (task-03) handles the API-layer translation only.
- Do not add `using Microsoft.EntityFrameworkCore` — this project must stay free of persistence concerns.
