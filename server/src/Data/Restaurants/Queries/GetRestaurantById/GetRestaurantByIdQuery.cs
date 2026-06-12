using CM.TableNow.Restaurants.Data.Queries.GetRestaurants;
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Restaurants.Data.Queries.GetRestaurantById;

public sealed record GetRestaurantByIdQuery(Guid Id) : IQuery<Result<RestaurantData>>;
