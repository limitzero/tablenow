# Task 01: Popular Restaurants Endpoint

## Status

pending

## Wave

1

## Description

Creates the query + handler for popular restaurants ranked by confirmed reservation count, with IMemoryCache caching. Registers `GET /api/restaurants/popular`.

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-003 AppDbContext, STORY-001 projects)
**Blocks:** Nothing (parallel with task-02)

## Files to Create

- `server/src/Data/Restaurants/Queries/GetPopularRestaurantsQuery.cs` (+ Handler)
- `server/src/Application/Restaurants/Features/GetPopularRestaurants/GetPopularRestaurantsRequest.cs` (+ Handler)
- `server/src/Contracts/Restaurants/PopularRestaurantDto.cs`

## Files to Modify

- `server/src/Api/Endpoints/RestaurantsEndpoints.cs` — add GET /popular

## Technical Details

### Code Snippets

```csharp
// PopularRestaurantDto.cs
public record PopularRestaurantDto(Guid Id, string Name, string Cuisine, string ThumbnailUrl, int BookingCount);
```

```csharp
// GetPopularRestaurantsQueryHandler.cs (with IMemoryCache)
public class GetPopularRestaurantsQueryHandler(AppDbContext db, IMemoryCache cache)
    : IRequestHandler<GetPopularRestaurantsQuery, Result<List<PopularRestaurantDto>>>
{
    public async ValueTask<Result<List<PopularRestaurantDto>>> Handle(
        GetPopularRestaurantsQuery query, CancellationToken ct)
    {
        var cacheKey = $"popular_{query.Period}";
        if (cache.TryGetValue(cacheKey, out List<PopularRestaurantDto>? cached))
            return Result<List<PopularRestaurantDto>>.Success(cached!);

        var since = query.Period == "week"
            ? DateTimeOffset.UtcNow.AddDays(-7)
            : DateTimeOffset.UtcNow.AddDays(-30);

        var results = await db.Reservations
            .Where(r => r.Status == "Confirmed" && r.CreatedAt >= since)
            .GroupBy(r => r.TimeSlot.RestaurantId)
            .Select(g => new { RestaurantId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .Join(db.Restaurants, x => x.RestaurantId, r => r.Id,
                (x, r) => new PopularRestaurantDto(r.Id, r.Name, r.Cuisine, r.ThumbnailUrl, x.Count))
            .ToListAsync(ct);

        cache.Set(cacheKey, results, TimeSpan.FromHours(1));
        return Result<List<PopularRestaurantDto>>.Success(results);
    }
}
```

```csharp
// Add to RestaurantsEndpoints (before /{id} to avoid routing conflict):
group.MapGet("/popular", async (
    [FromQuery] string period,
    IMediator mediator, CancellationToken ct) =>
{
    if (period is not "week" and not "month")
        return Results.BadRequest(new { error = "period must be 'week' or 'month'" });
    var result = await mediator.Send(new GetPopularRestaurantsRequest(period), ct);
    return TypedResultHelper.ToResult(result);
});
```

**Note:** Register `/popular` route BEFORE `/{id:guid}` to prevent the router from treating "popular" as a GUID.

Register `IMemoryCache` in `AddRestaurantsModule`: `services.AddMemoryCache()`.

## Acceptance Criteria

- [ ] `GET /api/restaurants/popular?period=week` returns top-10 by booking count
- [ ] Results cached 1 hour via IMemoryCache
- [ ] Invalid period returns 400
- [ ] No auth required
