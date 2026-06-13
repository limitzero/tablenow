using CM.TableNow.Auth.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CM.TableNow.Auth.Infrastructure.Extensions;

public static class AuthModuleExtensions
{
    public static IServiceCollection AddAuthModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // AuthDbContext is registered here (SQLite, "Default" connection string) to match the
        // Restaurants module. The data seeder resolves this context at startup.
        // TODO(STORY-005/006): register Auth handlers, password hasher, and JWT service.
        services.AddDbContext<AuthDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")));

        return services;
    }
}
