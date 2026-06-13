# Task 01: Popular Restaurants Backend

## Status

pending

## Wave

1

## Description

Add `GET /api/restaurants/popular?period=week|month&locale=` endpoint returning the top-N restaurants ranked by confirmed reservation count for the given period. Uses an EF aggregation query on the `Reservations` table. Results are cached in `IMemoryCache` with a 1-hour TTL to avoid repeated expensive aggregations.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** None

## Files to Create

- `server/src/Data/Reservations/Queries/GetPopularRestaurants/GetPopularRestaurantsQuery.cs` — Query + `PopularRestaurantData` + handler.
- `server/src/Application/Restaurants/Features/GetPopularRestaurants/GetPopularRestaurantsRequest.cs` — Request + handler (adds caching).
- `server/src/Api/Endpoints/Restaurants/PopularEndpoints.cs` — Endpoint.

## Technical Details

### Query

```csharp
public sealed record PopularRestaurantData(Guid RestaurantId, string Name, string Cuisine, string ThumbnailUrl, int BookingCount);

public sealed record GetPopularRestaurantsQuery(string Period, int TopN = 10) : IQuery<Result<IReadOnlyList<PopularRestaurantData>>>;
```

Handler: compute `startDate` from `Period` (`week` = last 7 days, `month` = last 30 days). Join `Reservations` with `Restaurants`, group by `RestaurantId`, count `Confirmed` reservations, order descending, take `TopN`.

### Caching

Application handler wraps the query result with `IMemoryCache` using key `$"popular:{period}"` and TTL of 1 hour:
```csharp
if (cache.TryGetValue(cacheKey, out var cached)) return Result.Success(cached);
var result = await mediator.Send(query, ct);
cache.Set(cacheKey, result.Data, TimeSpan.FromHours(1));
```

### Endpoint

```csharp
group.MapGet("/popular", async (string period = "week", IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(new GetPopularRestaurantsRequest(period), ct);
    return TypedResultHelper.ToResult(result);
});
```

## Acceptance Criteria

- [ ] `GET /api/restaurants/popular?period=week` returns restaurants ranked by booking count.
- [ ] Results are cached for 1 hour.
- [ ] Invalid `period` values return 400.
- [ ] Does not require authentication.
