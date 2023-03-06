using Microsoft.Extensions.DependencyInjection;
using Salus.HostedServices;
using Salus.Idempotency;
using Salus.Messaging;
using Salus.Saving;

namespace Salus;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Salus to a Service Collection.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type that the Salus instance belongs to.</typeparam>
    /// <param name="services">The Service Collection.</param>
    /// <param name="options">A <see cref="SalusOptions"/> object.</param>
    /// <returns></returns>
    public static IServiceCollection AddSalus<TContext, TKey>(this IServiceCollection services, Action<SalusOptions<TKey>> options) where TContext : SalusDbContext<TKey>
    {
        var optionsObject = new SalusOptions<TKey>();
        options(optionsObject);

        // Salus services are mostly transient - this ensures that each
        // DbContext instance gets its own Salus instance
        services.AddSingleton(optionsObject);
        services.AddTransient<ISalus<TKey>, SalusCore<TKey>>();
        services.AddTransient<IDbContextIdempotencyChecker, DbContextIdempotencyChecker>();
        services.AddTransient<IDbContextSaver<TKey>, DbContextSaver<TKey>>();
        services.AddTransient<IMessageSenderInternal<TKey>, MessageSenderInternal<TKey>>();

        services.AddScoped<IQueueProcessor<TContext, TKey>, QueueProcessor<TContext, TKey>>();

        services.AddHostedService<QueueProcessorService<TContext, TKey>>();

        return services;
    }
}