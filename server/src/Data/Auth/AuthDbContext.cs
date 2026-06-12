using CM.TableNow.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Auth.Data;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
}
