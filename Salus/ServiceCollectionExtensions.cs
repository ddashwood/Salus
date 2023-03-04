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
    public static IServiceCollection AddSalus<TContext>(this IServiceCollection services, Action<SalusOptions> options) where TContext : SalusDbContext
    {
        var optionsObject = new SalusOptions();
        options(optionsObject);

        // Salus services are mostly transient - this ensures that each
        // DbContext instance gets its own Salus instance
        services.AddSingleton(optionsObject);
        services.AddTransient<ISalus, SalusCore>();
        services.AddTransient<IDbContextIdempotencyChecker, DbContextIdempotencyChecker>();
        services.AddTransient<IDbContextSaver, DbContextSaver>();
        services.AddTransient<IMessageSenderInternal, MessageSenderInternal>();

        services.AddScoped<IQueueProcessor<TContext>, QueueProcessor<TContext>>();

        services.AddHostedService<QueueProcessorService<TContext>>();

        return services;
    }
}