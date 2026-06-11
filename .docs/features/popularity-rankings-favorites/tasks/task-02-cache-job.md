# Task 02: Popularity Cache / Scheduled Refresh Job

## Status

pending

## Wave

1

## Description

Adds `IMemoryCache` caching to the popular restaurants endpoint with a 1-hour TTL, or alternatively creates a `BackgroundService` that refreshes rankings daily. The cache prevents the aggregation query from running on every request.

## Dependencies

**Depends on:** STORY-001 task-03-api-startup.md
**Blocks:** task-03-home-section.md

## Files to Modify

- `server/src/Application/CM.TableNow.Reservations.Application/Features/GetPopularRestaurants/GetPopularRestaurantsRequestHandler.cs` — Add caching
- `server/src/Api/Extensions/ServiceCollectionExtensions.cs` — `services.AddMemoryCache()`

## Technical Details

```csharp
// In GetPopularRestaurantsRequestHandler — add IMemoryCache:
public class GetPopularRestaurantsRequestHandler(IMediator mediator, IMemoryCache cache) ...

public async ValueTask<Result<...>> Handle(...)
{
    var cacheKey = $"popular:{request.Period}:{request.Locale}";
    if (cache.TryGetValue(cacheKey, out var cached))
        return ResultExtensions.Ok(cached!);

    var result = await mediator.Send(query, ct);
    if (result.IsSuccess)
        cache.Set(cacheKey, result.Data, TimeSpan.FromHours(1));

    return result;
}
```

Add to `RegisterServices()`:
```csharp
services.AddMemoryCache();
```

## Acceptance Criteria

- [ ] Popularity results cached for 1 hour
- [ ] Second request within TTL served from cache (no DB query)
- [ ] `dotnet build` exits with code 0
