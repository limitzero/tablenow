using CM.TableNow.Restaurants.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CM.TableNow.Restaurants.Infrastructure.Extensions;

public static class RestaurantsModuleExtensions
{
    public static IServiceCollection AddRestaurantsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<RestaurantsDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")));

        return services;
    }
}
