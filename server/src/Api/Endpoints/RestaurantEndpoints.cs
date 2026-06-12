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

        restaurants.MapGet("/{id:guid}/slots", async (
            Guid id,
            DateOnly? date,
            int? partySize,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var errors = new Dictionary<string, string[]>();
            if (date is null)
                errors["date"] = ["date is required and must be in yyyy-MM-dd format."];
            if (partySize is null or <= 0)
                errors["partySize"] = ["partySize must be a positive integer."];

            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            var result = await mediator.Send(
                RestaurantMapper.ToGetAvailableSlotsRequest(id, date!.Value, partySize!.Value),
                ct);
            return TypedResultHelper.ToHttpResult(result);
        });

        return group;
    }
}
