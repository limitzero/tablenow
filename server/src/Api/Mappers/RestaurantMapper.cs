using CM.TableNow.Restaurants.Application.Features.GetAvailableSlots;
using CM.TableNow.Restaurants.Application.Features.GetRestaurantById;

namespace CM.TableNow.Api.Mappers;

public static class RestaurantMapper
{
    public static GetRestaurantByIdRequest ToGetByIdRequest(Guid id) => new(id);

    public static GetAvailableSlotsRequest ToGetAvailableSlotsRequest(Guid restaurantId, DateOnly date, int partySize)
        => new(restaurantId, date, partySize);
}
