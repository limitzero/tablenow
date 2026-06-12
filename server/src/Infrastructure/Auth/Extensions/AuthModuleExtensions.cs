using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CM.TableNow.Auth.Infrastructure.Extensions;

public static class AuthModuleExtensions
{
    public static IServiceCollection AddAuthModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO(STORY-005/006): register Auth DbContext, handlers, password hasher, JWT service.
        return services;
    }
}
