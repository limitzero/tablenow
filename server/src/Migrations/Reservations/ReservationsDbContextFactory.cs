using CM.TableNow.Reservations.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CM.TableNow.Reservations.Migrations;

internal sealed class ReservationsDbContextFactory : IDesignTimeDbContextFactory<ReservationsDbContext>
{
    public ReservationsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ReservationsDbContext>()
            .UseSqlite("Data Source=tablenow.dev.db",
                b => b.MigrationsAssembly("CM.TableNow.Reservations.Migrations"))
            .Options;
        return new ReservationsDbContext(options);
    }
}
