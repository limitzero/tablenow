using CM.TableNow.Reservations.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CM.TableNow.Reservations.Infrastructure.Extensions;

public static class ReservationsModuleExtensions
{
    public static IServiceCollection AddReservationsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO(STORY-014/016/017): register Reservations handlers and concurrency services.
        services.AddDbContext<ReservationsDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")));

        return services;
    }
}
