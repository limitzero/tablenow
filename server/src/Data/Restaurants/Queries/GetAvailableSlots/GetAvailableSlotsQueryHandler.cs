using CM.TableNow.Shared.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Restaurants.Data.Queries.GetAvailableSlots;

public sealed class GetAvailableSlotsQueryHandler(RestaurantsDbContext db)
    : IQueryHandler<GetAvailableSlotsQuery, Result<IReadOnlyList<SlotData>>>
{
    public async ValueTask<Result<IReadOnlyList<SlotData>>> Handle(
        GetAvailableSlotsQuery query,
        CancellationToken cancellationToken)
    {
        var startUtc = new DateTimeOffset(query.Date.Year, query.Date.Month, query.Date.Day, 0, 0, 0, TimeSpan.Zero);
        var endUtc = startUtc.AddDays(1);

        var slots = await db.TimeSlots
            .AsNoTracking()
            .Where(ts =>
                ts.RestaurantId == query.RestaurantId &&
                ts.StartTime >= startUtc &&
                ts.StartTime < endUtc &&
                ts.RemainingCapacity >= query.PartySize)
            .OrderBy(ts => ts.StartTime)
            .Select(ts => new SlotData(ts.Id, ts.StartTime, ts.RemainingCapacity))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<SlotData>>.Success(slots);
    }
}
