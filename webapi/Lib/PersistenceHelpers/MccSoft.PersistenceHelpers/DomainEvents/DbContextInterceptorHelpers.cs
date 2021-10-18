using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MccSoft.DomainHelpers.DomainEvents;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MccSoft.PersistenceHelpers.DomainEvents
{
    internal static class DbContextInterceptorHelpers
    {
        internal static List<IDomainEventEntity> GetChangedDomainEventEntitiesWithDomainEvents(
            DbContext dbContext,
            bool includeUnchanged = false
        ) {
            return dbContext.ChangeTracker.Entries<IDomainEventEntity>()
                .Where(
                    x =>
                        x.State != EntityState.Detached
                        && (includeUnchanged || x.State != EntityState.Unchanged)
                )
                .Select(po => po.Entity)
                .Where(po => po.DomainEvents?.Count > 0)
                .ToList();
        }

        internal static async Task ExecuteDomainEvents(
            DbContext context,
            IEnumerable<IDomainEventEntity> domainEventEntities,
            IMediator mediator,
            ILogger logger
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
                        await mediator.Publish(domainEvent);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(
                            e,
                            $"Error processing domain event {domainEvent.GetType()}"
                        );
                    }
                }

                entity.ClearDomainEvents();
            }
        }

        internal static T GetPrimaryKey<T>(DbContext context, object entity)
        {
            var entry = context.Entry(entity);
            object keyPart = entry.Metadata.FindPrimaryKey()
                .Properties.Select(p => entry.Property(p.Name).CurrentValue)
                .First();

            return (T)keyPart;
        }
    }
}
