using CM.TableNow.Api.Mappers;
using CM.TableNow.Restaurants.Application.Features.GetRestaurants;
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Api.Endpoints;

public static class RestaurantEndpoints
{
    public static RouteGroupBuilder MapRestaurantEndpoints(this RouteGroupBuilder group)
    {
        var restaurants = group.MapGroup("/restaurants");

        restaurants.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetRestaurantsRequest(), ct);
            return TypedResultHelper.ToHttpResult(result);
        });

        restaurants.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(RestaurantMapper.ToGetByIdRequest(id), ct);
            return TypedResultHelper.ToHttpResult(result);
        });

        return group;
    }
}
