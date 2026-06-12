using CM.TableNow.Contracts;
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Restaurants.Application.Features.GetAvailableSlots;

public sealed record GetAvailableSlotsRequest(
    Guid RestaurantId,
    DateOnly Date,
    int PartySize) : IRequest<Result<IReadOnlyList<TimeSlotResponse>>>;
