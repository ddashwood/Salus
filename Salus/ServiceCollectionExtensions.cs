using Microsoft.Extensions.DependencyInjection;
using Salus.QueueProcessing;
using Salus.Idempotency;
using Salus.Messaging;
using Salus.Saving;
using Microsoft.EntityFrameworkCore;
using Salus.Services;
using Salus.Purging;

namespace Salus;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Salus to a Service Collection.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type that the Salus instance belongs to.</typeparam>
    /// <param name="services">The Service Collection.</param>
    /// <param name="salusOptionsAction">A <see cref="SalusOptions"/> object.</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddSalus<TContext>(this IServiceCollection services,
        Action<SalusOptions<int>> salusOptionsAction, Action<DbContextOptionsBuilder>? contextOptionsAction = null) where TContext : SalusDbContext
    {
        var optionsObject = GetOptions(null);
        salusOptionsAction(optionsObject);

        AddSalus<TContext>(services, optionsObject, contextOptionsAction);

        return services;
    }


    public static IServiceCollection AddSalus<TContext>(this IServiceCollection services, IAsyncMessageSender asyncMessageSender,
        Action<SalusOptions<int>> salusOptionsAction, Action<DbContextOptionsBuilder>? contextOptionsAction = null) where TContext : SalusDbContext
    {
        ArgumentNullException.ThrowIfNull(asyncMessageSender);

        var optionsObject = GetOptions(asyncMessageSender);
        salusOptionsAction(optionsObject);

        AddSalus<TContext>(services, optionsObject, contextOptionsAction);

        return services;
    }


    private static SalusOptions<int> GetOptions(IAsyncMessageSender? asyncMessageSender)
    {
        return new SalusOptions<int>(asyncMessageSender);
    }

    private static void AddSalus<TContext>(IServiceCollection services, SalusOptions<int> optionsObject, Action<DbContextOptionsBuilder>? contextOptionsAction) where TContext : SalusDbContext
    {
        if (contextOptionsAction == null)
        {
            services.AddDbContext<TContext>();
        }
        else
        {
            services.AddDbContext<TContext>(contextOptionsAction);
        }

        // Several Salus services are transient - this ensures that each
        // DbContext instance gets its own Salus instance
        services.AddSingleton(optionsObject);
        services.AddTransient<ISalus<int>, SalusCore<int>>();
        services.AddTransient<IDbContextIdempotencyChecker, DbContextIdempotencyChecker>();
        services.AddTransient<IDbContextSaver<int>, DbContextSaver<int>>();
        services.AddTransient<IMessageSenderInternal<int>, MessageSenderInternal<int>>();

        services.AddScoped<ISalusDbContextProvider, SalusDbContextProvider>();

        // Hosted services, and their semaphores
        services.AddScoped<IQueueProcessor<TContext, int>, QueueProcessor<TContext, int>>();
        services.AddScoped<IPurger<TContext, int>, Purger<TContext, int>>();
        services.AddSingleton<IQueueProcessorSemaphore, QueueProcessorSemaphore>();
        services.AddSingleton<IPurgerSemaphore, PurgerSemaphore>();

        services.AddHostedService<QueueProcessorService<TContext, int>>();
        services.AddHostedService<PurgerService<TContext, int>>();
    }
}