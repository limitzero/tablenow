# Task 03: Reservation Endpoint & BDD Tests

## Status

pending

## Wave

2

## Description

Add the `POST /api/reservations` Minimal API endpoint that requires JWT authorization, extracts `userId` from the JWT `sub` claim, maps the API request body to `CreateReservationRequest`, and returns the appropriate HTTP status via `TypedResultHelper`. Also creates the `ReservationsMapper` static class and BDD unit tests for the capacity-exceeded scenario.

## Dependencies

**Depends on:** task-01-create-reservation-command.md, task-02-create-reservation-application.md
**Blocks:** None

**Context from dependencies:** task-01 defines `CreateReservationCommand` and `ReservationData`. task-02 defines `CreateReservationRequest(UserId, SlotId, PartySize) → Result<CreateReservationResponse(ReservationId, SlotId, PartySize, Status)>`. The route group `/api/reservations` is already protected by `.RequireAuthorization()` from STORY-007.

## Files to Create

- `server/src/Api/Endpoints/Reservations/ReservationsEndpoints.cs` — `static class ReservationsEndpoints` (if not yet created by STORY-007 placeholder).
- `server/src/Api/Endpoints/Reservations/ReservationsMapper.cs` — Static mapper.
- `server/src/Contracts/Reservations/CreateReservationRequest.cs` — API DTO.
- `server/tests/UnitTests/Reservations/describe_create_reservation.cs` — BDD tests.

## Files to Modify

- `server/src/Api/Program.cs` — Call `MapReservationsEndpoints`.

## Technical Details

### Implementation Steps

1. Define `CreateReservationRequest` API DTO with `SlotId` (Guid) and `PartySize` (int).
2. Create `ReservationsMapper.ToRequest(CreateReservationRequest body, Guid userId) => new Application.CreateReservationRequest(userId, body.SlotId, body.PartySize)`.
3. Add endpoint:
   ```csharp
   group.MapPost("/", async (
       CreateReservationRequest body,
       HttpContext ctx,
       IMediator mediator,
       CancellationToken ct) =>
   {
       var userId = Guid.Parse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
       var result = await mediator.Send(ReservationsMapper.ToRequest(body, userId), ct);
       return TypedResultHelper.ToResult(result);
   });
   ```
4. Write BDD tests in `describe_create_reservation`:
   ```csharp
   namespace describe_create_reservation;

   public class when_slot_is_fully_booked : module_fixture
   {
       [Fact]
       public async Task it_should_return_conflict_status()
       {
           // Arrange: mock mediator returns 409
           Mediator.Setup(m => m.Send(It.IsAny<CreateReservationRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(Result<CreateReservationResponse>.Failure(409, "This time slot is no longer available."));

           var handler = new CreateReservationRequestHandler(Mediator.Object);
           var result = await handler.Handle(
               new CreateReservationRequest(Guid.NewGuid(), Guid.NewGuid(), 2), default);

           result.StatusCode.Should().Be(409);
           result.IsSuccess.Should().BeFalse();
       }
   }
   ```

## Acceptance Criteria

- [ ] `POST /api/reservations` without a JWT returns 401 (enforced by route group authorization).
- [ ] `POST /api/reservations` with valid body and JWT returns 201 with `reservationId`, `slotId`, `partySize`, `status`.
- [ ] `POST /api/reservations` when slot is at capacity returns 409 with the error message.
- [ ] `userId` is extracted from `ctx.User` (JWT sub claim), not from the request body.
- [ ] BDD test `when_slot_is_fully_booked` → `it_should_return_conflict_status` passes.

## Notes

- `ClaimTypes.NameIdentifier` maps to the JWT `sub` claim when using `AddJwtBearer` with default mapping. If claims mapping is customized, adjust the claim type accordingly.
- `TypedResultHelper.ToResult` for a 201 response should use `Results.Created("/api/reservations/{id}", result.Data)`.
