using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Salus.Services;

public interface ISalusDbContextProvider
{
    DbContext GetDatabase(Type contextType, out IServiceScope newScope);
}