using CM.TableNow.Auth.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CM.TableNow.Auth.Migrations;

internal sealed class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseSqlite("Data Source=tablenow.dev.db",
                b => b.MigrationsAssembly("CM.TableNow.Auth.Migrations"))
            .Options;
        return new AuthDbContext(options);
    }
}
