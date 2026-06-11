# Task 04: Reservation Unit Tests

## Status

pending

## Wave

2

## Description

Writes BDD unit tests for the `CreateReservationRequestHandler`. Tests the 409 path (slot booked) and the 201 path (valid booking). Uses Moq and FluentAssertions. Does not overlap with task-03.

## Dependencies

**Depends on:** task-01-create-reservation-handler.md, task-02-create-reservation-command.md
**Blocks:** Nothing (tests are verification, not a dependency)

**Context from dependencies:** task-01 created `CreateReservationRequestHandler` that calls `IMediator.Send(CreateReservationCommand)`. task-02 created `CreateReservationCommand`. The unit tests mock IMediator to test the Application handler in isolation (not the Data handler). BDD test naming: namespace = `describe_create_reservation`, class = `when_slot_is_fully_booked`, method = `it_should_return_conflict_status`.

## Files to Create

- `server/tests/UnitTests/Reservations/describe_create_reservation/when_slot_is_fully_booked.cs`
- `server/tests/UnitTests/Reservations/describe_create_reservation/when_reservation_is_valid.cs`

## Technical Details

### Code Snippets

```csharp
// describe_create_reservation/when_slot_is_fully_booked.cs
namespace describe_create_reservation;

public class when_slot_is_fully_booked
{
    [Fact]
    public async Task it_should_return_conflict_status()
    {
        var mediator = new Mock<IMediator>();
        var httpContext = new Mock<IHttpContextAccessor>();
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("userId", Guid.NewGuid().ToString())]));
        httpContext.Setup(x => x.HttpContext!.User).Returns(claimsPrincipal);

        mediator.Setup(m => m.Send(It.IsAny<CreateReservationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ReservationCreatedDto>.Failure(
                "This time slot is no longer available.", 409));

        var handler = new CreateReservationRequestHandler(mediator.Object, httpContext.Object);
        var result = await handler.Handle(
            new CreateReservationRequest(Guid.NewGuid(), 2), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Errors.Should().Contain("This time slot is no longer available.");
    }
}
```

```csharp
// describe_create_reservation/when_reservation_is_valid.cs
namespace describe_create_reservation;

public class when_reservation_is_valid
{
    [Fact]
    public async Task it_should_return_created_status()
    {
        var reservationId = Guid.NewGuid();
        var slotId = Guid.NewGuid();
        var mediator = new Mock<IMediator>();
        var httpContext = new Mock<IHttpContextAccessor>();
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("userId", Guid.NewGuid().ToString())]));
        httpContext.Setup(x => x.HttpContext!.User).Returns(claimsPrincipal);

        mediator.Setup(m => m.Send(It.IsAny<CreateReservationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ReservationCreatedDto>.Success(
                new ReservationCreatedDto(reservationId, slotId, "Test Restaurant",
                    DateTimeOffset.UtcNow.AddDays(1), 2, "Confirmed")));

        var handler = new CreateReservationRequestHandler(mediator.Object, httpContext.Object);
        var result = await handler.Handle(
            new CreateReservationRequest(slotId, 2), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data!.ReservationId.Should().Be(reservationId);
    }
}
```

## Acceptance Criteria

- [ ] `when_slot_is_fully_booked.it_should_return_conflict_status` passes
- [ ] `when_reservation_is_valid.it_should_return_created_status` passes
- [ ] Tests mock IMediator and IHttpContextAccessor (no real DB or HTTP context)
- [ ] BDD namespace `describe_create_reservation`, lowercase with underscores
