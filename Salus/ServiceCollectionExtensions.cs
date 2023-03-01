using Microsoft.Extensions.DependencyInjection;
using Salus.Idempotency;
using Salus.Saving;

namespace Salus;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSalus(this IServiceCollection services)
    {
        services.AddScoped<ISalusCore, SalusCore>();
        services.AddScoped<IDbContextIdempotencyChecker, DbContextIdempotencyChecker>();
        services.AddScoped<IDbContextSaver, DbContextSaver>();

        return services;
    }
}