using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CM.TableNow.Restaurants.Infrastructure.Extensions;

public static class RestaurantsModuleExtensions
{
    public static IServiceCollection AddRestaurantsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO(STORY-010/011): register Restaurants DbContext, handlers, query services.
        return services;
    }
}
