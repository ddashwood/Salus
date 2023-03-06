using Microsoft.Extensions.DependencyInjection;
using Salus.QueueProcessing;
using Salus.Idempotency;
using Salus.Messaging;
using Salus.Saving;
using System.Net.NetworkInformation;

namespace Salus;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Salus to a Service Collection.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type that the Salus instance belongs to.</typeparam>
    /// <param name="services">The Service Collection.</param>
    /// <param name="options">A <see cref="SalusOptions"/> object.</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddSalus<TContext>(this IServiceCollection services, Action<SalusOptions<int>> options) where TContext : SalusDbContext
    {
        var optionsObject = GetOptions(null, null);
        options(optionsObject);

        AddSalus<TContext>(services, optionsObject);

        return services;
    }

    public static IServiceCollection AddSalus<TContext>(this IServiceCollection services, IMessageSender messageSender, Action<SalusOptions<int>> options) where TContext : SalusDbContext
    {
        ArgumentNullException.ThrowIfNull(messageSender);

        var optionsObject = GetOptions(messageSender, null);
        options(optionsObject);

        AddSalus<TContext>(services, optionsObject);

        return services;
    }

    public static IServiceCollection AddSalus<TContext>(this IServiceCollection services, IMessageSender messageSender, IAsyncMessageSender asyncMessageSender, Action<SalusOptions<int>> options) where TContext : SalusDbContext
    {
        ArgumentNullException.ThrowIfNull(messageSender);
        ArgumentNullException.ThrowIfNull(asyncMessageSender);

        var optionsObject = GetOptions(messageSender, asyncMessageSender);
        options(optionsObject);

        AddSalus<TContext>(services, optionsObject);

        return services;
    }


    private static SalusOptions<int> GetOptions(IMessageSender? messageSender, IAsyncMessageSender? asyncMessageSender)
    {
        return new SalusOptions<int>(messageSender, asyncMessageSender);
    }

    private static void AddSalus<TContext>(IServiceCollection services, SalusOptions<int> optionsObject) where TContext : SalusDbContext
    {
        // Salus services are mostly transient - this ensures that each
        // DbContext instance gets its own Salus instance
        services.AddSingleton(optionsObject);
        services.AddTransient<ISalus<int>, SalusCore<int>>();
        services.AddTransient<IDbContextIdempotencyChecker, DbContextIdempotencyChecker>();
        services.AddTransient<IDbContextSaver<int>, DbContextSaver<int>>();
        services.AddTransient<IMessageSenderInternal<int>, MessageSenderInternal<int>>();

        services.AddScoped<IQueueProcessor<TContext, int>, QueueProcessor<TContext, int>>();
        services.AddSingleton<IQueueProcessorSemaphore, QueueProcessorSemaphore>();

        services.AddHostedService<QueueProcessorService<TContext, int>>();
    }
}