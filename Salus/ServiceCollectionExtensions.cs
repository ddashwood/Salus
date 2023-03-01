using Microsoft.Extensions.DependencyInjection;
using Salus.Idempotency;
using Salus.Saving;

namespace Salus;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSalus(this IServiceCollection services, Action<SalusOptions> options)
    {
        var optionsObject = new SalusOptions();
        options(optionsObject);

        // Salus services are mostly transient - this ensures that each
        // DbContext instance gets its own Salus instance
        services.AddSingleton(optionsObject);
        services.AddTransient<ISalusCore, SalusCore>();
        services.AddTransient<IDbContextIdempotencyChecker, DbContextIdempotencyChecker>();
        services.AddTransient<IDbContextSaver, DbContextSaver>();

        return services;
    }
}