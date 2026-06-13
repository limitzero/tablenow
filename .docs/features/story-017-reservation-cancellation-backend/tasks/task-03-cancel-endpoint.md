# Task 03: Cancel Endpoint & BDD Tests

## Status

pending

## Wave

2

## Description

Add the `DELETE /api/reservations/{id}` Minimal API endpoint that extracts the `userId` from the JWT `sub` claim and dispatches `CancelReservationRequest`. Returns 200 on success, 403 for a non-owner attempt, 404 for unknown reservation, and 409 for double cancellation. Also adds BDD tests for the non-owner forbidden case.

## Dependencies

**Depends on:** task-01-cancel-reservation-command.md, task-02-cancel-reservation-application.md
**Blocks:** None

**Context from dependencies:** task-02 defines `CancelReservationRequest(ReservationId, RequestingUserId) → Result<Unit>`. task-01 defines the status code contract (403/404/409/200). The `ReservationsEndpoints.cs` already exists from STORY-014 — this task adds the DELETE route to the same class.

## Files to Create

- `server/tests/UnitTests/Reservations/describe_cancel_reservation.cs` — BDD test class.

## Files to Modify

- `server/src/Api/Endpoints/Reservations/ReservationsEndpoints.cs` — Add `DELETE /{id}` route.
- `server/src/Api/Endpoints/Reservations/ReservationsMapper.cs` — Add `ToCancelRequest(Guid reservationId, Guid userId)` mapper.

## Technical Details

### Implementation Steps

1. Add mapper: `public static CancelReservationRequest ToCancelRequest(Guid reservationId, Guid userId) => new(reservationId, userId)`.
2. Add DELETE route to the route group:
   ```csharp
   group.MapDelete("/{id:guid}", async (
       Guid id,
       HttpContext ctx,
       IMediator mediator,
       CancellationToken ct) =>
   {
       var userId = Guid.Parse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
       var result = await mediator.Send(ReservationsMapper.ToCancelRequest(id, userId), ct);
       return TypedResultHelper.ToResult(result);
   });
   ```
3. Write BDD tests:
   ```csharp
   namespace describe_cancel_reservation;

   public class when_user_is_not_owner : module_fixture
   {
       [Fact]
       public async Task it_should_return_forbidden()
       {
           Mediator.Setup(m => m.Send(It.IsAny<CancelReservationRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(Result<Unit>.Failure(403, "Forbidden"));

           var handler = new CancelReservationRequestHandler(Mediator.Object);
           var result = await handler.Handle(
               new CancelReservationRequest(Guid.NewGuid(), Guid.NewGuid()), default);

           result.StatusCode.Should().Be(403);
           result.IsSuccess.Should().BeFalse();
       }
   }
   ```

## Acceptance Criteria

- [ ] `DELETE /api/reservations/{id}` by the owner returns 200.
- [ ] `DELETE /api/reservations/{id}` by a different user returns 403.
- [ ] `DELETE /api/reservations/{id}` for an already-cancelled reservation returns 409.
- [ ] BDD test `when_user_is_not_owner` → `it_should_return_forbidden` passes.
