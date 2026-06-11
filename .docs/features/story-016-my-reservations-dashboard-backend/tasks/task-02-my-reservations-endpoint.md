# Task 02: My Reservations Endpoint

## Status

pending

## Wave

2

## Description

Adds `GET /my` to `ReservationsEndpoints.cs`. Reads userId from JWT claims and delegates to `GetMyReservationsRequest`.

## Dependencies

**Depends on:** task-01-get-my-reservations-handler.md
**Blocks:** STORY-018 (frontend dashboard calls this)

**Context from dependencies:** task-01 created `GetMyReservationsRequest` and handler. STORY-014 task-03 created `ReservationsEndpoints.cs`. This task adds one route to that existing class.

## Files to Modify

- `server/src/Api/Endpoints/ReservationsEndpoints.cs`

## Technical Details

### Code Snippets

```csharp
// Add inside MapReservationsEndpoints():
group.MapGet("/my", async (
    HttpContext httpContext,
    IMediator mediator,
    CancellationToken ct) =>
{
    var result = await mediator.Send(new GetMyReservationsRequest(), ct);
    return TypedResultHelper.ToResult(result);
});
```

Note: userId is read inside `GetMyReservationsRequestHandler` via `IHttpContextAccessor` — not passed via route.

## Acceptance Criteria

- [ ] `GET /api/reservations/my` mapped in `ReservationsEndpoints`
- [ ] Requires auth (inherited from the group's `.RequireAuthorization()`)
- [ ] Returns 200 with `[]` when no reservations
- [ ] Returns 401 without JWT
