using CM.TableNow.Contracts;
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Restaurants.Application.Features.GetRestaurants;

public sealed record GetRestaurantsRequest : IRequest<Result<IReadOnlyList<RestaurantResponse>>>;
