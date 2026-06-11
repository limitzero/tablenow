# Task 02: My Reservations Application Handler & Endpoint

## Status

pending

## Wave

2

## Description

Creates `GetMyReservationsRequest` / handler and `GET /api/reservations/my` endpoint. Reads `userId` from JWT claims and passes to the data query. Auth required.

## Dependencies

**Depends on:** task-01-my-reservations-query.md
**Blocks:** STORY-018 (frontend dashboard calls this endpoint)

**Context from dependencies:** task-01 created `GetMyReservationsQuery(UserId)` returning `IReadOnlyList<MyReservationDto>`. DTO: `ReservationId, RestaurantName, Date, Time, PartySize, Status`.

## Files to Create

- `server/src/Application/CM.TableNow.Reservations.Application/Features/GetMyReservations/GetMyReservationsRequest.cs`
- `server/src/Application/CM.TableNow.Reservations.Application/Features/GetMyReservations/GetMyReservationsRequestHandler.cs`

## Files to Modify

- `server/src/Api/Endpoints/ReservationEndpoints.cs` — Add `GET /reservations/my`

## Technical Details

### Code Snippets

```csharp
// Add to ReservationEndpoints.MapReservationEndpoints():
reservations.MapGet("/my", async (
    ClaimsPrincipal user,
    IMediator mediator,
    CancellationToken ct) =>
{
    var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await mediator.Send(new GetMyReservationsRequest(userId), ct);
    return TypedResultHelper.ToResult(result);
})
.WithName("GetMyReservations")
.Produces<IReadOnlyList<MyReservationResponse>>(200)
.ProducesProblem(401);
```

Response DTO for the API:
```csharp
public record MyReservationResponse(
    Guid ReservationId, string RestaurantName,
    string Date, string Time, int PartySize, string Status);
```

## Acceptance Criteria

- [ ] `GET /api/reservations/my` with valid JWT returns 200 with reservations array
- [ ] Returns empty array `[]` when user has no reservations
- [ ] 401 for unauthenticated requests
- [ ] `userId` from JWT `sub` claim only
- [ ] `dotnet build` exits with code 0
