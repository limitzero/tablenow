using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Restaurants.Data.Queries.GetRestaurants;

public sealed record RestaurantData(
    Guid Id,
    string Name,
    string Cuisine,
    string Address,
    string Description,
    string ThumbnailUrl);

public sealed record GetRestaurantsQuery : IQuery<Result<IReadOnlyList<RestaurantData>>>;
