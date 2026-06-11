# Task 03: Reservation Endpoint

## Status

pending

## Wave

2

## Description

Creates `ReservationsEndpoints.cs` and registers `POST /api/reservations`. The endpoint requires auth (in the route group configured by STORY-007). Maps `Result` to `IResult` via `TypedResultHelper` — the 409 case returns the specific conflict message.

## Dependencies

**Depends on:** task-01-create-reservation-handler.md, task-02-create-reservation-command.md
**Blocks:** STORY-015 (frontend booking flow), STORY-016, STORY-017 (other reservation endpoints go in this class)

**Context from dependencies:** task-01 created `CreateReservationRequest` and `CreateReservationResponse`. STORY-007 configured the `/api/reservations` route group with `.RequireAuthorization()`. This task creates the endpoint class and adds `POST /` to it. Does not overlap with task-04.

## Files to Create

- `server/src/Api/Endpoints/ReservationsEndpoints.cs`

## Files to Modify

- `server/src/Api/Program.cs` — call `MapReservationsEndpoints()` on the reservations group

## Technical Details

### Code Snippets

```csharp
// server/src/Api/Endpoints/ReservationsEndpoints.cs
namespace TableNow.Api.Endpoints;

public static class ReservationsEndpoints
{
    public static RouteGroupBuilder MapReservationsEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (
            CreateReservationRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(request, ct);
            return TypedResultHelper.ToResult(result);
        });

        // GET /my added in STORY-016
        // DELETE /{id} added in STORY-017
        return group;
    }
}
```

Note: `userId` is read inside `CreateReservationRequestHandler` from `IHttpContextAccessor` — the endpoint itself does not need to extract it.

```csharp
// Program.cs update (in STORY-007, the group was already set up):
app.MapGroup("/api/reservations")
   .MapReservationsEndpoints()
   .RequireAuthorization();
```

## Acceptance Criteria

- [ ] `ReservationsEndpoints.cs` exists with `MapReservationsEndpoints` method
- [ ] `POST /` endpoint mapped with no explicit auth attribute (auth is on the group from STORY-007)
- [ ] Endpoint sends `CreateReservationRequest` via IMediator and returns `TypedResultHelper.ToResult`
- [ ] Program.cs maps the reservations group with `.RequireAuthorization()`
