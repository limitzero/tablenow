using CM.TableNow.Restaurants.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Restaurants.Data;

public sealed class RestaurantsDbContext(DbContextOptions<RestaurantsDbContext> options) : DbContext(options)
{
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(RestaurantsDbContext).Assembly);
}
