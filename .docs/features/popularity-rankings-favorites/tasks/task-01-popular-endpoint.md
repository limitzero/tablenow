# Task 01: Popular Restaurants Aggregation Query & Endpoint

## Status

pending

## Wave

1

## Description

Implements `GET /api/restaurants/popular?period=week|month&locale=` with an EF aggregation query on confirmed reservations. Returns top-10 restaurants ranked by booking count.

## Dependencies

**Depends on:** STORY-014 task-04-endpoint.md (Reservations table has data)
**Blocks:** task-03-home-section.md

## Files to Create

- `server/src/Data/CM.TableNow.Reservations.Data/Queries/GetPopularRestaurants/GetPopularRestaurantsQuery.cs`
- `server/src/Data/CM.TableNow.Reservations.Data/Queries/GetPopularRestaurants/GetPopularRestaurantsQueryHandler.cs`
- `server/src/Application/CM.TableNow.Reservations.Application/Features/GetPopularRestaurants/GetPopularRestaurantsRequest.cs`
- `server/src/Application/CM.TableNow.Reservations.Application/Features/GetPopularRestaurants/GetPopularRestaurantsRequestHandler.cs`

## Files to Modify

- `server/src/Api/Endpoints/RestaurantEndpoints.cs` — Add `/popular` route

## Technical Details

```csharp
// GetPopularRestaurantsQueryHandler.cs
public async ValueTask<IReadOnlyList<PopularRestaurantDto>> Handle(
    GetPopularRestaurantsQuery query, CancellationToken ct)
{
    var cutoff = query.Period == "week"
        ? DateTime.UtcNow.AddDays(-7) : DateTime.UtcNow.AddMonths(-1);

    var topIds = await reservationsDb.Reservations
        .Where(r => r.Status == "Confirmed" && r.CreatedAt >= cutoff)
        .GroupBy(r => r.TimeSlotId)
        .Select(g => new { TimeSlotId = g.Key, Count = g.Count() })
        .OrderByDescending(x => x.Count)
        .Take(10)
        .ToListAsync(ct);

    // Join with restaurant data via separate query (cross-context)
    // ... fetch restaurant names from RestaurantsDbContext
}
```

### API Endpoint

```
GET /api/restaurants/popular?period=week&locale=
200: [{ restaurantId, name, cuisine, bookingCount, thumbnailUrl }]
```

## Acceptance Criteria

- [ ] Endpoint returns top restaurants by confirmed booking count
- [ ] `period=week` filters last 7 days; `period=month` filters last 30 days
- [ ] Results ordered by booking count descending
- [ ] `dotnet build` exits with code 0
