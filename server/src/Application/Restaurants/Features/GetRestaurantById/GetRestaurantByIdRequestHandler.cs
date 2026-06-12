using CM.TableNow.Contracts;
using CM.TableNow.Restaurants.Data.Queries.GetRestaurantById;
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Restaurants.Application.Features.GetRestaurantById;

public sealed class GetRestaurantByIdRequestHandler(IMediator mediator)
    : IRequestHandler<GetRestaurantByIdRequest, Result<RestaurantResponse>>
{
    public async ValueTask<Result<RestaurantResponse>> Handle(
        GetRestaurantByIdRequest request,
        CancellationToken cancellationToken)
    {
        var dataResult = await mediator.Send(new GetRestaurantByIdQuery(request.Id), cancellationToken);

        if (!dataResult.IsSuccess)
            return Result<RestaurantResponse>.Failure(dataResult.StatusCode, [.. dataResult.Errors]);

        var d = dataResult.Data!;
        return Result<RestaurantResponse>.Success(
            new RestaurantResponse(d.Id, d.Name, d.Cuisine, d.Address, d.Description, d.ThumbnailUrl));
    }
}
