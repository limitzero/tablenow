# Task 02: CreateReservation Application Layer

## Status

pending

## Wave

1

## Description

Implement the Application-layer `CreateReservationRequestHandler` that validates the reservation request and dispatches the `CreateReservationCommand` to the Data layer. The `userId` is not taken from the request body — it is passed in from the endpoint (which extracts it from the JWT claims). The handler validates that `partySize >= 1` and delegates all business logic to the Data command. Returns `Result<CreateReservationResponse>`.

## Dependencies

**Depends on:** None (Wave 1) — the contract for `CreateReservationCommand` is stable enough to code against in parallel with task-01.
**Blocks:** task-03-reservation-endpoint.md

**Context from dependencies:** task-01 defines `CreateReservationCommand(UserId, SlotId, PartySize) : ICommand<Result<ReservationData>>` where `ReservationData(ReservationId, SlotId, PartySize, Status)`. The Application layer dispatches this command via `IMediator` and maps the result to `CreateReservationResponse`.

## Files to Create

- `server/src/Application/Reservations/Features/CreateReservation/CreateReservationRequest.cs` — Request and Response records.
- `server/src/Application/Reservations/Features/CreateReservation/CreateReservationRequestHandler.cs` — Handler.

## Files to Modify

- None.

## Technical Details

### Implementation Steps

1. Define `CreateReservationRequest(Guid UserId, Guid SlotId, int PartySize)` implementing `IRequest<Result<CreateReservationResponse>>`.
2. Define `CreateReservationResponse(Guid ReservationId, Guid SlotId, int PartySize, string Status)`.
3. Implement handler with primary constructor `(IMediator mediator)`.
4. Validate: `if (request.PartySize < 1) return Result.Failure(400, "Party size must be at least 1")`.
5. Dispatch `CreateReservationCommand(request.UserId, request.SlotId, request.PartySize)`.
6. On failure, propagate the result status code and errors.
7. On success, map `ReservationData` to `CreateReservationResponse` and return `Result.Success(...)`.

### Code Snippets

```csharp
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Reservations.Application.Features.CreateReservation;

public sealed record CreateReservationRequest(Guid UserId, Guid SlotId, int PartySize)
    : IRequest<Result<CreateReservationResponse>>;

public sealed record CreateReservationResponse(
    Guid ReservationId,
    Guid SlotId,
    int PartySize,
    string Status);
```

## Acceptance Criteria

- [ ] Handler returns 400 when `PartySize < 1`.
- [ ] Handler dispatches `CreateReservationCommand` with the correct `UserId`, `SlotId`, `PartySize`.
- [ ] Handler propagates 409 from the Data layer unchanged.
- [ ] Handler returns 201-compatible `Result<CreateReservationResponse>` on success.

## Notes

- `UserId` is passed from the endpoint (JWT claim extraction) — the Application handler never reads HTTP context directly.
