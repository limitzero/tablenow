using CM.TableNow.Contracts;
using CM.TableNow.Restaurants.Data.Queries.GetRestaurants;
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Restaurants.Application.Features.GetRestaurants;

public sealed class GetRestaurantsRequestHandler(IMediator mediator)
    : IRequestHandler<GetRestaurantsRequest, Result<IReadOnlyList<RestaurantResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<RestaurantResponse>>> Handle(
        GetRestaurantsRequest request,
        CancellationToken cancellationToken)
    {
        var dataResult = await mediator.Send(new GetRestaurantsQuery(), cancellationToken);

        if (!dataResult.IsSuccess)
            return Result<IReadOnlyList<RestaurantResponse>>.Failure(dataResult.StatusCode, [.. dataResult.Errors]);

        IReadOnlyList<RestaurantResponse> responses = dataResult.Data!
            .Select(d => new RestaurantResponse(d.Id, d.Name, d.Cuisine, d.Address, d.Description, d.ThumbnailUrl))
            .ToList();

        return Result<IReadOnlyList<RestaurantResponse>>.Success(responses);
    }
}
