using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CM.TableNow.Reservations.Infrastructure.Extensions;

public static class ReservationsModuleExtensions
{
    public static IServiceCollection AddReservationsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO(STORY-014/016/017): register Reservations DbContext, handlers, concurrency services.
        return services;
    }
}
