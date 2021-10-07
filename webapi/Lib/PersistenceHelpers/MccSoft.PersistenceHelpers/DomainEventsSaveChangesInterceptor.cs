using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MccSoft.DomainHelpers.DomainEvents;
using MccSoft.DomainHelpers.DomainEvents.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MccSoft.PersistenceHelpers
{
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
        ) {
            _mediator = mediator;
            _logger = logger;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = new CancellationToken()
        ) {
            _domainEventEntities = GetChangedDomainEventEntitiesWithDomainEvents(eventData.Context);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result
        ) {
            _domainEventEntities = GetChangedDomainEventEntitiesWithDomainEvents(eventData.Context);

            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = new()
        ) {
            await ExecuteDomainEvents(eventData.Context, _domainEventEntities);

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            ExecuteDomainEvents(eventData.Context, _domainEventEntities).GetAwaiter().GetResult();

            return base.SavedChanges(eventData, result);
        }

        private List<IDomainEventEntity> GetChangedDomainEventEntitiesWithDomainEvents(
            DbContext dbContext
        ) {
            return dbContext.ChangeTracker.Entries<IDomainEventEntity>()
                .Where(x => x.State != EntityState.Detached && x.State != EntityState.Unchanged)
                .Select(po => po.Entity)
                .Where(po => po.DomainEvents?.Count > 0)
                .ToList();
        }

        private async Task ExecuteDomainEvents(
            DbContext context,
            IEnumerable<IDomainEventEntity> domainEventEntities
        ) {
            foreach (var entity in domainEventEntities)
            {
                var events = entity.DomainEvents.ToList();
                foreach (var domainEvent in events)
                {
                    if (domainEvent is IDomainEventWithIntegerId domainEventWithIntegerId)
                    {
                        domainEventWithIntegerId.Id = GetPrimaryKey<int>(context, entity);
                    }

                    if (domainEvent is IDomainEventWithStringId domainEventWithStringId)
                    {
                        domainEventWithStringId.Id = GetPrimaryKey<object>(context, entity)
                            .ToString();
                    }

                    try
                    {
                        await _mediator.Publish(domainEvent);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(
                            e,
                            $"Error processing domain event {domainEvent.GetType()}"
                        );
                    }
                }

                entity.ClearDomainEvents();
            }
        }

        private T GetPrimaryKey<T>(DbContext context, object entity)
        {
            var entry = context.Entry(entity);
            object keyPart = entry.Metadata.FindPrimaryKey()
                .Properties.Select(p => entry.Property(p.Name).CurrentValue)
                .First();

            return (T)keyPart;
        }
    }
}
