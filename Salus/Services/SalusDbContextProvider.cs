using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Salus.Services;

internal class SalusDbContextProvider : ISalusDbContextProvider
{
    private readonly IServiceProvider _serviceProvider;

    public SalusDbContextProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public DbContext GetDatabase(Type contextType, out IServiceScope newScope)
    {
        newScope = _serviceProvider.CreateScope();
        return (DbContext)newScope.ServiceProvider.GetRequiredService(contextType);
    }
}
