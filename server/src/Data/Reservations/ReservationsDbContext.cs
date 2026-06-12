using CM.TableNow.Reservations.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Reservations.Data;

public sealed class ReservationsDbContext(DbContextOptions<ReservationsDbContext> options) : DbContext(options)
{
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReservationsDbContext).Assembly);
}
