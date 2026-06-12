using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Restaurants.Data.Queries.GetAvailableSlots;

public sealed record SlotData(
    Guid SlotId,
    DateTimeOffset Time,
    int RemainingCapacity);

public sealed record GetAvailableSlotsQuery(
    Guid RestaurantId,
    DateOnly Date,
    int PartySize) : IQuery<Result<IReadOnlyList<SlotData>>>;
