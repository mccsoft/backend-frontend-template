using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MccSoft.DomainHelpers.DomainEvents;
using MccSoft.DomainHelpers.DomainEvents.Events;
using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MccSoft.PersistenceHelpers.DomainEvents;

/// <summary>
/// Interceptor to handle <see cref="LogDomainEvent"/> emitted from entities.
/// Should be added to DbContext via:
/// .AddDbContext<TemplateAppDbContext>((provider, opt) => opt
///       .UseNpgsql(...)
///       .AddInterceptors(provider.GetRequiredService<DomainEventsSaveChangesInterceptor>())
/// )
/// </summary>
public class DomainEventsSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IMediator _mediator;
    private readonly ILogger<DomainEventsSaveChangesInterceptor> _logger;
    private List<IDomainEventEntity> _domainEventEntities;

    public DomainEventsSaveChangesInterceptor(
        IMediator mediator,
        ILogger<DomainEventsSaveChangesInterceptor> logger
    )
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken()
    )
    {
        if (eventData.Context.Database.CurrentTransaction == null)
        {
            _domainEventEntities =
                DbContextInterceptorHelpers.GetChangedDomainEventEntitiesWithDomainEvents(
                    eventData.Context
                );
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        if (eventData.Context.Database.CurrentTransaction == null)
        {
            _domainEventEntities =
                DbContextInterceptorHelpers.GetChangedDomainEventEntitiesWithDomainEvents(
                    eventData.Context
                );
        }

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = new()
    )
    {
        if (eventData.Context.Database.CurrentTransaction == null)
        {
            await DbContextInterceptorHelpers.ExecuteDomainEvents(
                eventData.Context,
                _domainEventEntities,
                _mediator,
                _logger
            );
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (eventData.Context.Database.CurrentTransaction == null)
        {
            DbContextInterceptorHelpers
                .ExecuteDomainEvents(eventData.Context, _domainEventEntities, _mediator, _logger)
                .GetAwaiter()
                .GetResult();
        }

        return base.SavedChanges(eventData, result);
    }
}
