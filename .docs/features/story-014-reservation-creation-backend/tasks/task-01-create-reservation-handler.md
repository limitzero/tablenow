# Task 01: Create Reservation Handler (Application Layer)

## Status

pending

## Wave

1

## Description

Creates the Application-layer types for reservation creation: the request, response, handler, and validator. The handler reads `userId` from JWT claims via `IHttpContextAccessor` and delegates the actual DB work to the Data command via IMediator.

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-001 projects, STORY-003 domain entities)
**Blocks:** task-03-reservation-endpoint.md, task-04-reservation-unit-tests.md

**Context from dependencies:** STORY-001 created `Application.Reservations` project. STORY-003 created the `Reservation` domain entity. `Result<T>` from STORY-001. The `userId` is extracted from `HttpContext.User` claims (not from request body). This task is parallel to task-02 — they are in different projects and different file paths.

## Files to Create

- `server/src/Application/Reservations/Features/CreateReservation/CreateReservationRequest.cs`
- `server/src/Application/Reservations/Features/CreateReservation/CreateReservationResponse.cs`
- `server/src/Application/Reservations/Features/CreateReservation/CreateReservationRequestHandler.cs`
- `server/src/Application/Reservations/Features/CreateReservation/CreateReservationRequestValidator.cs`

## Technical Details

### Code Snippets

```csharp
// CreateReservationRequest.cs
namespace TableNow.Application.Reservations.Features.CreateReservation;

public record CreateReservationRequest(Guid SlotId, int PartySize)
    : IRequest<Result<CreateReservationResponse>>;
```

```csharp
// CreateReservationResponse.cs
namespace TableNow.Application.Reservations.Features.CreateReservation;

public record CreateReservationResponse(
    Guid ReservationId,
    Guid SlotId,
    string RestaurantName,
    DateTimeOffset DateTime,
    int PartySize,
    string Status);
```

```csharp
// CreateReservationRequestHandler.cs
namespace TableNow.Application.Reservations.Features.CreateReservation;

public class CreateReservationRequestHandler(
    IMediator mediator,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<CreateReservationRequest, Result<CreateReservationResponse>>
{
    public async ValueTask<Result<CreateReservationResponse>> Handle(
        CreateReservationRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst("userId")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Result<CreateReservationResponse>.Failure("Unauthorized.", 401);

        var command = new CreateReservationCommand(userId, request.SlotId, request.PartySize);
        var result = await mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Result<CreateReservationResponse>.Success(
                new CreateReservationResponse(
                    result.Data!.ReservationId,
                    result.Data.SlotId,
                    result.Data.RestaurantName,
                    result.Data.DateTime,
                    result.Data.PartySize,
                    result.Data.Status), 201)
            : Result<CreateReservationResponse>.Failure(result.Errors, result.StatusCode);
    }
}
```

```csharp
// CreateReservationRequestValidator.cs
public class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(x => x.SlotId).NotEmpty();
        RuleFor(x => x.PartySize).InclusiveBetween(1, 20);
    }
}
```

## Acceptance Criteria

- [ ] `CreateReservationRequest` has `SlotId` (Guid) and `PartySize` (int)
- [ ] Handler reads `userId` from JWT claim "userId" via `IHttpContextAccessor`
- [ ] Handler sends `CreateReservationCommand` via IMediator (no direct EF access)
- [ ] Returns 401 if userId claim is missing
- [ ] Validator rejects empty SlotId and partySize outside 1–20
