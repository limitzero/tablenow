using CM.TableNow.Restaurants.Application.Features.GetRestaurantById;

namespace CM.TableNow.Api.Mappers;

public static class RestaurantMapper
{
    public static GetRestaurantByIdRequest ToGetByIdRequest(Guid id) => new(id);
}
