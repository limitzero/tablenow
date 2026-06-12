using CM.TableNow.Restaurants.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CM.TableNow.Restaurants.Migrations;

internal sealed class RestaurantsDbContextFactory : IDesignTimeDbContextFactory<RestaurantsDbContext>
{
    public RestaurantsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<RestaurantsDbContext>()
            .UseSqlite("Data Source=tablenow.dev.db",
                b => b.MigrationsAssembly("CM.TableNow.Restaurants.Migrations"))
            .Options;
        return new RestaurantsDbContext(options);
    }
}
