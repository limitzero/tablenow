# Task 03: Application Handler

## Status

pending

## Wave

3

## Description

Implements `CreateReservationRequest` / `CreateReservationRequestHandler` in the Application/Reservations layer. Validates the request, reads `userId` from the mediator context (passed from the endpoint), dispatches `CreateReservationCommand`, and returns `Result<CreateReservationResponse>`.

## Dependencies

**Depends on:** task-02-create-command.md
**Blocks:** task-04-endpoint.md

**Context from dependencies:** task-02 created `CreateReservationCommand(UserId, TimeSlotId, PartySize)` returning `Result<Guid>`. The Result from the command is 201 or 409.

## Files to Create

- `server/src/Application/CM.TableNow.Reservations.Application/Features/CreateReservation/CreateReservationRequest.cs`
- `server/src/Application/CM.TableNow.Reservations.Application/Features/CreateReservation/CreateReservationResponse.cs`
- `server/src/Application/CM.TableNow.Reservations.Application/Features/CreateReservation/CreateReservationRequestHandler.cs`

## Technical Details

### Code Snippets

```csharp
// CreateReservationRequest.cs
public record CreateReservationRequest(Guid UserId, Guid TimeSlotId, int PartySize)
    : IRequest<Result<CreateReservationResponse>>;

public record CreateReservationResponse(Guid ReservationId, string Status);
```

```csharp
// CreateReservationRequestHandler.cs
public class CreateReservationRequestHandler(IMediator mediator)
    : IRequestHandler<CreateReservationRequest, Result<CreateReservationResponse>>
{
    public async ValueTask<Result<CreateReservationResponse>> Handle(
        CreateReservationRequest request,
        CancellationToken ct)
    {
        if (request.PartySize < 1)
            return ResultExtensions.BadRequest<CreateReservationResponse>("Party size must be at least 1.");

        var commandResult = await mediator.Send(
            new CreateReservationCommand(request.UserId, request.TimeSlotId, request.PartySize), ct);

        if (!commandResult.IsSuccess)
            return ResultExtensions.Fail<CreateReservationResponse>(
                commandResult.StatusCode, commandResult.Errors.ToArray());

        return ResultExtensions.Created(new CreateReservationResponse(commandResult.Data, "Confirmed"));
    }
}
```

## Acceptance Criteria

- [ ] Handler exists at specified path
- [ ] Forwards command result status codes (409) to caller
- [ ] Returns 201 with `{ reservationId, status: "Confirmed" }` on success
- [ ] `dotnet build` exits with code 0
