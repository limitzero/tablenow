# Task 02: Cancel Reservation Application Handler & Endpoint

## Status

pending

## Wave

2

## Description

Creates `CancelReservationRequest` / handler with ownership check, and `DELETE /api/reservations/{id}` endpoint. Returns 403 if JWT user is not the reservation owner. BDD test for the unauthorized case.

## Dependencies

**Depends on:** task-01-cancel-command.md
**Blocks:** STORY-018 (cancel button in dashboard calls this)

**Context from dependencies:** task-01 created `CancelReservationCommand(ReservationId)` returning `Result<CancelledReservationResult>` where `CancelledReservationResult` includes `OwnerId`. The application handler compares the `OwnerId` to the JWT `userId` to enforce ownership.

## Files to Create

- `server/src/Application/CM.TableNow.Reservations.Application/Features/CancelReservation/CancelReservationRequest.cs`
- `server/src/Application/CM.TableNow.Reservations.Application/Features/CancelReservation/CancelReservationRequestHandler.cs`
- `server/tests/UnitTests/Reservations/describe_cancel_reservation/when_user_is_not_owner.cs`

## Files to Modify

- `server/src/Api/Endpoints/ReservationEndpoints.cs` — Add `DELETE /reservations/{id}`

## Technical Details

### Code Snippets

```csharp
// CancelReservationRequest.cs
public record CancelReservationRequest(Guid ReservationId, Guid RequestingUserId)
    : IRequest<Result<Unit>>;
```

```csharp
// CancelReservationRequestHandler.cs
public class CancelReservationRequestHandler(IMediator mediator)
    : IRequestHandler<CancelReservationRequest, Result<Unit>>
{
    public async ValueTask<Result<Unit>> Handle(
        CancelReservationRequest request, CancellationToken ct)
    {
        var commandResult = await mediator.Send(
            new CancelReservationCommand(request.ReservationId), ct);

        if (!commandResult.IsSuccess)
            return ResultExtensions.Fail<Unit>(commandResult.StatusCode, commandResult.Errors.ToArray());

        // Ownership check — compare JWT userId with reservation owner
        if (commandResult.Data!.OwnerId != request.RequestingUserId)
            return ResultExtensions.Forbidden<Unit>();

        return ResultExtensions.Ok(Unit.Value);
    }
}
```

Add to `ReservationEndpoints`:
```csharp
reservations.MapDelete("/{id:guid}", async (
    Guid id,
    ClaimsPrincipal user,
    IMediator mediator,
    CancellationToken ct) =>
{
    var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await mediator.Send(new CancelReservationRequest(id, userId), ct);
    return TypedResultHelper.ToResult(result);
})
.WithName("CancelReservation")
.Produces(200)
.ProducesProblem(403)
.ProducesProblem(409);
```

BDD test:
```csharp
namespace describe_cancel_reservation;

public class when_user_is_not_owner : module_fixture
{
    [Fact]
    public async Task it_should_return_forbidden()
    {
        Mediator.Send(Arg.Any<CancelReservationCommand>(), Arg.Any<CancellationToken>())
            .Returns(ResultExtensions.Ok(new CancelledReservationResult(
                Guid.NewGuid(), Guid.NewGuid(), 2))); // owner != requester

        var handler = new CancelReservationRequestHandler(Mediator);
        var result = await handler.Handle(
            new CancelReservationRequest(Guid.NewGuid(), Guid.NewGuid()), // different user
            CancellationToken.None);

        result.StatusCode.Should().Be(403);
    }
}
```

## Acceptance Criteria

- [ ] Owner can cancel → 200
- [ ] Non-owner → 403
- [ ] Already cancelled → 409
- [ ] BDD test `describe_cancel_reservation` / `when_user_is_not_owner` passes
- [ ] `dotnet test` passes
