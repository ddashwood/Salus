using Microsoft.EntityFrameworkCore;
using Salus.Models.Changes;

namespace Salus;

/// <summary>
/// Represents the core internal behaviour of Salus
/// </summary>
internal interface ISalusCore
{
    /// <summary>
    /// The <see cref="SalusOptions"/> for this instsance of Salus.
    /// </summary>
    SalusOptions Options { get; }


    /// <summary>
    /// Initialise the instance by supplying the context to which it is attached.
    /// </summary>
    /// <typeparam name="TContext">The context type.</typeparam>
    /// <param name="context">The context.</param>
    void Init<TContext>(TContext context) where TContext : DbContext, ISalusDbContext;

    /// <summary>
    /// Internal handling of SaveChanges() calls
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">
    ///     Indicates whether <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges" />
    ///     is called after the changes have been sent successfully to the database.
    /// </param>
    /// <param name="baseSaveChanges">The base DbContext's SaveChanges method.</param>
    /// <returns>The number of state entries written to the database.</returns>
    int SaveChanges(bool acceptAllChangesOnSuccess, Func<bool, int> baseSaveChanges);

    /// <summary>
    /// Internal handling of SaveChangesAsync() calls.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">
    ///     Indicates whether <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges" />
    ///     is called after the changes have been sent successfully to the database.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <param name="baseSaveChanges">The base DbContext's SaveChanges method.</param>
    /// <returns>
    ///     A task that represents the asynchronous save operation. The task result contains the
    ///     number of state entries written to the database.
    /// </returns>
    Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken, Func<bool, CancellationToken, Task<int>> baseSaveChanges);

    /// <summary>
    /// Applies a <see cref="Save"/> to the database.
    /// </summary>
    /// <param name="save">The Save to apply.</param>
    void Apply(Save save);

    /// <summary>
    /// Configures the model.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    void OnModelCreating(ModelBuilder modelBuilder);
    
    /// <summary>
    /// Sends a message to consumers with details of a <see cref="Save"/>.
    /// </summary>
    /// <param name="save">The Save to communicate to consumers.</param>
    void SendMessages(Save save);

    /// <summary>
    /// Sends a message to consumers with details of a <see cref="Save"/>.
    /// </summary>
    /// <param name="save">The Save to communicate to consumers.</param>
    /// <returns>A Task which represents the asynchronous send operation.</returns>
    Task SendMessageAsync(Save save);
}
