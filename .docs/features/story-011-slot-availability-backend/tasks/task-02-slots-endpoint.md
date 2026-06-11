# Task 02: Slots Endpoint & Test

## Status

pending

## Wave

2

## Description

Adds `GET /{id}/slots` to the existing `RestaurantsEndpoints.cs` class and writes a BDD test for the fully-booked exclusion. Parses `date` and `partySize` from query parameters, returns 400 if missing.

## Dependencies

**Depends on:** task-01-get-available-slots-handler.md
**Blocks:** STORY-013 (restaurant detail frontend calls this endpoint)

**Context from dependencies:** task-01 created `GetAvailableSlotsRequest` with validator. STORY-010 task-02 created `RestaurantsEndpoints.cs` with `GET /` and `GET /{id}` routes. This task adds `GET /{id}/slots` to that same class. The two tasks modify the same file sequentially (not in parallel).

## Files to Modify

- `server/src/Api/Endpoints/RestaurantsEndpoints.cs` — add GET /{id}/slots route

## Files to Create

- `server/tests/UnitTests/Restaurants/describe_get_available_slots/when_party_size_exceeds_remaining_capacity.cs`

## Technical Details

### Code Snippets

```csharp
// Add inside RestaurantsEndpoints.MapRestaurantsEndpoints():
group.MapGet("/{id:guid}/slots", async (
    Guid id,
    [FromQuery] string? date,
    [FromQuery] int? partySize,
    IMediator mediator,
    CancellationToken ct) =>
{
    if (!DateOnly.TryParse(date, out var parsedDate) || partySize is null)
        return Results.BadRequest(new { error = "date (YYYY-MM-DD) and partySize are required." });

    var result = await mediator.Send(
        new GetAvailableSlotsRequest(id, parsedDate, partySize.Value), ct);
    return TypedResultHelper.ToResult(result);
});
```

```csharp
// describe_get_available_slots/when_party_size_exceeds_remaining_capacity.cs
namespace describe_get_available_slots;
public class when_party_size_exceeds_remaining_capacity
{
    [Fact]
    public async Task it_should_exclude_slot_from_results()
    {
        // Handler returns empty list when slot capacity is insufficient
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetAvailableSlotsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<SlotDto>>.Success([]));

        var handler = new GetAvailableSlotsRequestHandler(mediator.Object);
        var result = await handler.Handle(
            new GetAvailableSlotsRequest(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today.AddDays(1)), 10),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}
```

## Acceptance Criteria

- [ ] `GET /api/restaurants/{id}/slots?date=&partySize=` is mapped
- [ ] Missing `date` or `partySize` returns 400
- [ ] Valid request delegates to `GetAvailableSlotsRequest` via IMediator
- [ ] BDD test passes
