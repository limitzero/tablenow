using CM.TableNow.Contracts;
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Restaurants.Application.Features.GetRestaurantById;

public sealed record GetRestaurantByIdRequest(Guid Id) : IRequest<Result<RestaurantResponse>>;
