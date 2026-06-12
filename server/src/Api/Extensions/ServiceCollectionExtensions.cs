using CM.TableNow.Auth.Infrastructure.Extensions;
using CM.TableNow.Reservations.Infrastructure.Extensions;
using CM.TableNow.Restaurants.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace CM.TableNow.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);

        services
            .AddAuthModule(configuration)
            .AddRestaurantsModule(configuration)
            .AddReservationsModule(configuration);

        return services;
    }
}
