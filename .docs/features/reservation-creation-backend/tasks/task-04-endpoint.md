# Task 04: Reservation Endpoint

## Status

pending

## Wave

4

## Description

Wires `POST /api/reservations` to `CreateReservationRequestHandler`. Reads `userId` from `ClaimsPrincipal` and passes it to the request. Applies `RequireAuthorization()`. Registers the Reservations module.

## Dependencies

**Depends on:** task-03-app-handler.md
**Blocks:** STORY-015 (frontend calls this endpoint), STORY-016 (uses same endpoint group)

**Context from dependencies:** task-03 created `CreateReservationRequest(UserId, TimeSlotId, PartySize)` and `CreateReservationResponse(ReservationId, Status)`. STORY-007 configured JWT bearer authentication and `RequireAuthorization()` support.

## Files to Create

- `server/src/Api/Endpoints/ReservationEndpoints.cs`
- `server/src/Api/Mappers/ReservationMapper.cs`
- `server/src/Contracts/Reservations/CreateReservationApiRequest.cs`
- `server/src/Application/CM.TableNow.Reservations.Application/Extensions/ServiceCollectionExtensions.cs`

## Files to Modify

- `server/src/Api/Extensions/ServiceCollectionExtensions.cs` — Uncomment `services.AddReservationsModule()`
- `server/src/Api/Program.cs` — Map reservation endpoints with auth requirement

## Technical Details

### Code Snippets

```csharp
// ReservationEndpoints.cs
public static class ReservationEndpoints
{
    public static RouteGroupBuilder MapReservationEndpoints(this RouteGroupBuilder group)
    {
        var reservations = group.MapGroup("/reservations").RequireAuthorization();

        reservations.MapPost("/", async (
            CreateReservationApiRequest apiRequest,
            ClaimsPrincipal user,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var request = new CreateReservationRequest(userId, apiRequest.TimeSlotId, apiRequest.PartySize);
            var result = await mediator.Send(request, ct);
            return TypedResultHelper.ToResult(result);
        })
        .WithName("CreateReservation")
        .Produces<CreateReservationResponse>(201)
        .ProducesProblem(401)
        .ProducesProblem(409);

        return group;
    }
}
```

Update `Program.cs`:
```csharp
app.MapGroup("/api")
   .MapAuthEndpoints()
   .MapRestaurantEndpoints()
   .MapReservationEndpoints();
```

`CreateReservationApiRequest`:
```csharp
public record CreateReservationApiRequest(Guid TimeSlotId, int PartySize);
```

## Acceptance Criteria

- [ ] `POST /api/reservations` without JWT returns 401
- [ ] `POST /api/reservations` with valid JWT and valid slot returns 201
- [ ] `POST /api/reservations` with full slot returns 409
- [ ] `userId` read from JWT `sub` claim — never from request body
- [ ] `dotnet build` exits with code 0
