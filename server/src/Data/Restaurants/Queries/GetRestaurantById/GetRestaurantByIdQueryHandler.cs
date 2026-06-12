using CM.TableNow.Restaurants.Data.Queries.GetRestaurants;
using CM.TableNow.Shared.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Restaurants.Data.Queries.GetRestaurantById;

public sealed class GetRestaurantByIdQueryHandler(RestaurantsDbContext db)
    : IQueryHandler<GetRestaurantByIdQuery, Result<RestaurantData>>
{
    public async ValueTask<Result<RestaurantData>> Handle(
        GetRestaurantByIdQuery query,
        CancellationToken cancellationToken)
    {
        var restaurant = await db.Restaurants
            .AsNoTracking()
            .Where(r => r.Id == query.Id)
            .Select(r => new RestaurantData(
                r.Id,
                r.Name,
                r.Cuisine,
                r.Address.Street + ", " + r.Address.City + ", " + r.Address.Region + " " + r.Address.PostalCode,
                r.Description,
                r.ThumbnailUrl))
            .FirstOrDefaultAsync(cancellationToken);

        return restaurant is null
            ? Result<RestaurantData>.NotFound("Restaurant not found.")
            : Result<RestaurantData>.Success(restaurant);
    }
}
