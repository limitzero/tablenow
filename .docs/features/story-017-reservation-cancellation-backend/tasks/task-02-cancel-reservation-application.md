# Task 02: CancelReservation Application Layer

## Status

pending

## Wave

1

## Description

Implement the Application-layer `CancelReservationRequestHandler` that accepts the reservation ID and the requesting user's ID (extracted from the JWT at the endpoint), dispatches `CancelReservationCommand` to the Data layer, and maps the result. This handler is thin — all business logic (ownership check, double-cancel guard, atomic update) lives in the Data command.

## Dependencies

**Depends on:** None (Wave 1) — the contract for `CancelReservationCommand` is stable enough to code against in parallel with task-01.
**Blocks:** task-03-cancel-endpoint.md

**Context from dependencies:** task-01 defines `CancelReservationCommand(ReservationId, RequestingUserId) : ICommand<Result<Unit>>`. The handler dispatches this via `IMediator` and propagates the status (200, 403, 404, 409) unchanged.

## Files to Create

- `server/src/Application/Reservations/Features/CancelReservation/CancelReservationRequest.cs` — Request record.
- `server/src/Application/Reservations/Features/CancelReservation/CancelReservationRequestHandler.cs` — Handler.

## Files to Modify

- None.

## Technical Details

### Implementation Steps

1. Define `CancelReservationRequest(Guid ReservationId, Guid RequestingUserId)` implementing `IRequest<Result<Unit>>`.
2. Implement handler with primary constructor `(IMediator mediator)`.
3. Dispatch `CancelReservationCommand(request.ReservationId, request.RequestingUserId)` via mediator.
4. Return the result as-is (status codes 200, 403, 404, 409 all propagate).

### Code Snippets

```csharp
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Reservations.Application.Features.CancelReservation;

public sealed record CancelReservationRequest(
    Guid ReservationId,
    Guid RequestingUserId) : IRequest<Result<Unit>>;

public sealed class CancelReservationRequestHandler(IMediator mediator)
    : IRequestHandler<CancelReservationRequest, Result<Unit>>
{
    public async ValueTask<Result<Unit>> Handle(
        CancelReservationRequest request,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(
            new CancelReservationCommand(request.ReservationId, request.RequestingUserId),
            cancellationToken);
    }
}
```

## Acceptance Criteria

- [ ] Handler dispatches `CancelReservationCommand` with the correct IDs.
- [ ] Handler propagates 403, 404, 409, and 200 results unchanged.
- [ ] No business logic in the Application handler — it is a pure dispatch layer.
