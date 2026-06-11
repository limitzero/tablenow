# Task 02: Cancel Endpoint & Tests

## Status

pending

## Wave

2

## Description

Adds `DELETE /{id}` to `ReservationsEndpoints.cs` and writes two BDD tests: not-owner (403) and already-cancelled (409).

## Dependencies

**Depends on:** task-01-cancel-reservation-handler.md
**Blocks:** STORY-018 (frontend cancel button calls this)

**Context from dependencies:** task-01 created `CancelReservationRequest`. STORY-014/016 created `ReservationsEndpoints.cs`. This task adds the DELETE route and two test files.

## Files to Modify

- `server/src/Api/Endpoints/ReservationsEndpoints.cs`

## Files to Create

- `server/tests/UnitTests/Reservations/describe_cancel_reservation/when_user_is_not_owner.cs`
- `server/tests/UnitTests/Reservations/describe_cancel_reservation/when_reservation_already_cancelled.cs`

## Technical Details

### Code Snippets

```csharp
// Add to MapReservationsEndpoints():
group.MapDelete("/{id:guid}", async (
    Guid id,
    IMediator mediator,
    CancellationToken ct) =>
{
    var result = await mediator.Send(new CancelReservationRequest(id), ct);
    return TypedResultHelper.ToResult(result);
});
```

```csharp
// when_user_is_not_owner.cs
namespace describe_cancel_reservation;
public class when_user_is_not_owner
{
    [Fact]
    public async Task it_should_return_forbidden()
    {
        var mediator = new Mock<IMediator>();
        var httpContext = new Mock<IHttpContextAccessor>();
        SetupClaims(httpContext, Guid.NewGuid());

        mediator.Setup(m => m.Send(It.IsAny<CancelReservationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Failure("Forbidden.", 403));

        var handler = new CancelReservationRequestHandler(mediator.Object, httpContext.Object);
        var result = await handler.Handle(new CancelReservationRequest(Guid.NewGuid()), CancellationToken.None);

        result.StatusCode.Should().Be(403);
    }

    private static void SetupClaims(Mock<IHttpContextAccessor> mock, Guid userId)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("userId", userId.ToString())]));
        mock.Setup(x => x.HttpContext!.User).Returns(principal);
    }
}
```

## Acceptance Criteria

- [ ] `DELETE /api/reservations/{id}` mapped in `ReservationsEndpoints`
- [ ] `when_user_is_not_owner.it_should_return_forbidden` passes
- [ ] `when_reservation_already_cancelled.it_should_return_conflict` passes
