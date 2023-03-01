using Microsoft.Extensions.DependencyInjection;
using Salus.Idempotency;
using Salus.Saving;

namespace Salus;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSalus(this IServiceCollection services)
    {
        // Salus services are all transient - this ensures that each
        // DbContext instance gets its own Salus instance
        services.AddTransient<ISalusCore, SalusCore>();
        services.AddTransient<IDbContextIdempotencyChecker, DbContextIdempotencyChecker>();
        services.AddTransient<IDbContextSaver, DbContextSaver>();

        return services;
    }
}