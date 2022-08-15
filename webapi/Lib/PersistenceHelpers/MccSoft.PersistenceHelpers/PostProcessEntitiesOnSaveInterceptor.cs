using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MccSoft.PersistenceHelpers;

/// <summary>
/// Interceptor that allows to execute arbitrary code when entities of certain types are saved.
/// Helpful to set something like OrganisationId/TenantId for all created/modified entities
/// to be equal to id of current user.
///
/// To integrate it add something like the following to OnConfiguring:
///  optionsBuilder.AddInterceptors(
///     new PostProcessEntitiesOnSaveInterceptor<IOwnedEntity, TemplateAppDbContext>(
///         (entity, context) =>
///         {
///             entity.SetOwnerIdUnsafe(context.CurrentOwnerId);
///         }
///     )
///   );
/// </summary>
public class PostProcessEntitiesOnSaveInterceptor<TEntity, TDbContext> : SaveChangesInterceptor
    where TDbContext : DbContext
{
    private readonly Action<TEntity, TDbContext> _postProcessAction;

    public PostProcessEntitiesOnSaveInterceptor(Action<TEntity, TDbContext> postProcessAction)
    {
        _postProcessAction = postProcessAction;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken()
    )
    {
        return new ValueTask<InterceptionResult<int>>(SavingChanges(eventData, result));
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        var context = eventData.Context;
        var entitiesToPostProcess = context.ChangeTracker
            .Entries()
            .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified)
            .Select(x => x.Entity)
            .OfType<TEntity>()
            .ToList();

        if (entitiesToPostProcess.Any())
        {
            foreach (var entity in entitiesToPostProcess)
            {
                _postProcessAction.Invoke(entity, (TDbContext)context);
            }
        }

        return base.SavingChanges(eventData, result);
    }
}
