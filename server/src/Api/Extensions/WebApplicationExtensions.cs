using CM.TableNow.Auth.Data;
using CM.TableNow.Auth.Data.Seed;
using CM.TableNow.Restaurants.Data;
using CM.TableNow.Restaurants.Data.Seed;

namespace CM.TableNow.Api.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Runs the database seeders for the Auth and Restaurants modules at startup. Both seeders
    /// are idempotent, so calling this on every boot is safe and will not duplicate records.
    /// </summary>
    public static async Task SeedDatabaseAsync(
        this WebApplication app,
        CancellationToken cancellationToken = default)
    {
        // A dedicated scope is required because DbContext is registered as scoped and cannot be
        // resolved from the root (singleton) service provider.
        await using var scope = app.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;

        var authDb = services.GetRequiredService<AuthDbContext>();
        await AuthDataSeeder.SeedAsync(authDb, cancellationToken);

        var restaurantsDb = services.GetRequiredService<RestaurantsDbContext>();
        await RestaurantDataSeeder.SeedAsync(restaurantsDb, cancellationToken);
        await TimeSlotDataSeeder.SeedAsync(restaurantsDb, cancellationToken);
    }
}
