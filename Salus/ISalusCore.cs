using Microsoft.EntityFrameworkCore;
using Salus.Models.Changes;

namespace Salus;

internal interface ISalusCore
{
    void Init<TContext>(TContext context) where TContext : DbContext, ISalusDbContext;

    Save? BuildPreliminarySave();
    Task<Save?> BuildPreliminarySaveAsync(CancellationToken cancellationToken);
    void CompleteSave(Save save);
    Task CompleteSaveAsync(Save save);

    void Apply(Save save);

    void Check(ModelBuilder modelBuilder);
    
    void SendMessages(Save save);

    SalusOptions Options { get; }
}
