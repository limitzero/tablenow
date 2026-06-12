# Task 03: Slot Availability Endpoint

## Status

complete

## Wave

2

## Description

Wire up the public `GET /api/restaurants/{id}/slots?date=&partySize=` Minimal API endpoint, extend `RestaurantMapper` with a `ToGetAvailableSlotsRequest` factory, and add BDD unit tests for the Data-layer query handler. The endpoint validates both query parameters inline (returning a structured 400 if either is missing or invalid), dispatches through the Application layer via Mediator, and returns the result via `TypedResultHelper`. No authentication is required on this route.

## Dependencies

**Depends on:** task-01-slot-availability-data-query.md, task-02-slot-availability-application-layer.md
**Blocks:** None

**Context from dependencies:** Task-01 created `GetAvailableSlotsQuery(RestaurantId, Date, PartySize)` in `CM.TableNow.Restaurants.Data.Queries.GetAvailableSlots` and `GetAvailableSlotsQueryHandler` which filters `TimeSlots` by date window and `RemainingCapacity >= partySize`. Task-02 created `GetAvailableSlotsRequest(RestaurantId, Date, PartySize)` in `CM.TableNow.Restaurants.Application.Features.GetAvailableSlots` and `GetAvailableSlotsRequestHandler` which dispatches to task-01's query and maps `SlotData` → `TimeSlotResponse`. This task adds the HTTP surface and tests.

## Files to Modify

- `server/src/Api/Endpoints/RestaurantEndpoints.cs` — add `GET /{id:guid}/slots` route with validation logic.
- `server/src/Api/Mappers/RestaurantMapper.cs` — add `ToGetAvailableSlotsRequest` static factory method.
- `server/tests/UnitTests/describe_get_available_slots.cs` — BDD unit tests for the Data-layer handler.

## Technical Details

### Implementation Steps

1. In `RestaurantEndpoints.MapRestaurantEndpoints`, add a third `MapGet` for `"/{id:guid}/slots"` that accepts `Guid id`, `DateOnly? date`, `int? partySize`, `IMediator mediator`, and `CancellationToken ct` as route/query parameters.
2. Validate inline: collect missing/invalid fields into a `Dictionary<string, string[]>` errors map. Return `Results.ValidationProblem(errors)` immediately if the map is non-empty.
3. If valid, call `RestaurantMapper.ToGetAvailableSlotsRequest(id, date!.Value, partySize!.Value)` and send via `mediator.Send(...)`.
4. Return `TypedResultHelper.ToHttpResult(result)`.
5. Add `ToGetAvailableSlotsRequest(Guid restaurantId, DateOnly date, int partySize)` to `RestaurantMapper` returning `new GetAvailableSlotsRequest(restaurantId, date, partySize)`.
6. Write BDD unit tests using SQLite in-memory: two test classes — `when_party_size_exceeds_remaining_capacity` (asserts empty result) and `when_slot_has_sufficient_capacity` (asserts the slot is returned with correct `SlotId` and `RemainingCapacity`). Both use `IAsyncLifetime` for setup/teardown.

### Code Snippets

**Endpoint addition inside `MapRestaurantEndpoints`:**

```csharp
restaurants.MapGet("/{id:guid}/slots", async (
    Guid id,
    DateOnly? date,
    int? partySize,
    IMediator mediator,
    CancellationToken ct) =>
{
    var errors = new Dictionary<string, string[]>();
    if (date is null)
        errors["date"] = ["date is required and must be in yyyy-MM-dd format."];
    if (partySize is null or <= 0)
        errors["partySize"] = ["partySize must be a positive integer."];

    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    var result = await mediator.Send(
        RestaurantMapper.ToGetAvailableSlotsRequest(id, date!.Value, partySize!.Value),
        ct);
    return TypedResultHelper.ToHttpResult(result);
});
```

**Mapper addition:**

```csharp
public static GetAvailableSlotsRequest ToGetAvailableSlotsRequest(Guid restaurantId, DateOnly date, int partySize)
    => new(restaurantId, date, partySize);
```

**BDD unit tests (SQLite in-memory):**

```csharp
namespace describe_get_available_slots;

public class when_party_size_exceeds_remaining_capacity : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private RestaurantsDbContext _db = null!;
    private Guid _restaurantId;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();
        var options = new DbContextOptionsBuilder<RestaurantsDbContext>()
            .UseSqlite(_connection).Options;
        _db = new RestaurantsDbContext(options);
        await _db.Database.EnsureCreatedAsync();

        _restaurantId = Guid.NewGuid();
        _db.Restaurants.Add(new Restaurant { Id = _restaurantId, Name = "Test", Cuisine = "Italian",
            Address = new Address("1 Main St", "Springfield", "IL", "62701") });
        _db.TimeSlots.Add(new TimeSlot { Id = Guid.NewGuid(), RestaurantId = _restaurantId,
            StartTime = new DateTimeOffset(2026, 6, 20, 18, 0, 0, TimeSpan.Zero),
            TotalCapacity = 10, RemainingCapacity = 2 });
        await _db.SaveChangesAsync();
    }

    public async Task DisposeAsync() { await _db.DisposeAsync(); await _connection.DisposeAsync(); }

    [Fact]
    public async Task it_should_not_return_the_slot()
    {
        var handler = new GetAvailableSlotsQueryHandler(_db);
        var result = await handler.Handle(
            new GetAvailableSlotsQuery(_restaurantId, new DateOnly(2026, 6, 20), partySize: 4),
            CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}

public class when_slot_has_sufficient_capacity : IAsyncLifetime
{
    // ... similar setup with RemainingCapacity = 6, partySize = 4
    // Asserts: result.Data has count 1, SlotId matches, RemainingCapacity == 6
}
```

### API Endpoints

- `GET /api/restaurants/{id}/slots?date=yyyy-MM-dd&partySize=N`
  - 200: `TimeSlotResponse[]` — `[{ slotId, time, remainingCapacity }, ...]`
  - 400: `ValidationProblemDetails` — when `date` is missing or `partySize` is missing/non-positive
  - 404 (propagated from Application): when `restaurantId` doesn't exist (handled by `TypedResultHelper`)

## Acceptance Criteria

- [ ] `GET /api/restaurants/{id}/slots?date=2026-06-20&partySize=4` returns 200 with matching slots.
- [ ] Missing `date` query param returns 400 with `errors.date` message.
- [ ] Missing or `<= 0` `partySize` query param returns 400 with `errors.partySize` message.
- [ ] `RestaurantMapper.ToGetAvailableSlotsRequest` exists and correctly constructs `GetAvailableSlotsRequest`.
- [ ] BDD test `when_party_size_exceeds_remaining_capacity` asserts empty result for a slot with `RemainingCapacity = 2` and `partySize = 4`.
- [ ] BDD test `when_slot_has_sufficient_capacity` asserts the slot is returned with correct `SlotId` and `RemainingCapacity`.
- [ ] Both BDD tests use SQLite in-memory via `IAsyncLifetime`.

## Notes

- `DateOnly?` is parsed automatically by ASP.NET Core's minimal API binder from query strings in `yyyy-MM-dd` format.
- The endpoint does not require `.RequireAuthorization()` — slots are public reads.
- `TypedResultHelper.ToHttpResult` handles the `Result<T>` → `IResult` translation consistently with other endpoints in the project.
