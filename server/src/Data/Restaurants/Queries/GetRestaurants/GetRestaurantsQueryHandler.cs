using CM.TableNow.Shared.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Restaurants.Data.Queries.GetRestaurants;

public sealed class GetRestaurantsQueryHandler(RestaurantsDbContext db)
    : IQueryHandler<GetRestaurantsQuery, Result<IReadOnlyList<RestaurantData>>>
{
    public async ValueTask<Result<IReadOnlyList<RestaurantData>>> Handle(
        GetRestaurantsQuery query,
        CancellationToken cancellationToken)
    {
        var restaurants = await db.Restaurants
            .AsNoTracking()
            .Select(r => new RestaurantData(
                r.Id,
                r.Name,
                r.Cuisine,
                r.Address.Street + ", " + r.Address.City + ", " + r.Address.Region + " " + r.Address.PostalCode,
                r.Description,
                r.ThumbnailUrl))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<RestaurantData>>.Success(restaurants);
    }
}
