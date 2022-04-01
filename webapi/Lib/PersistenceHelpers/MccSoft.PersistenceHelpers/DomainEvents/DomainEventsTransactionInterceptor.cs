using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using MccSoft.DomainHelpers.DomainEvents;
using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MccSoft.PersistenceHelpers.DomainEvents
{
    public class DomainEventsTransactionInterceptor : DbTransactionInterceptor
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DomainEventsTransactionInterceptor> _logger;

        public DomainEventsTransactionInterceptor(
            IMediator mediator,
            ILogger<DomainEventsTransactionInterceptor> logger
        )
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override void TransactionCommitted(
            DbTransaction transaction,
            TransactionEndEventData eventData
        )
        {
            List<IDomainEventEntity> domainEventEntities =
                DbContextInterceptorHelpers.GetChangedDomainEventEntitiesWithDomainEvents(
                    eventData.Context,
                    true
                );
            DbContextInterceptorHelpers
                .ExecuteDomainEvents(eventData.Context, domainEventEntities, _mediator, _logger)
                .GetAwaiter()
                .GetResult();

            base.TransactionCommitted(transaction, eventData);
        }

        public override async Task TransactionCommittedAsync(
            DbTransaction transaction,
            TransactionEndEventData eventData,
            CancellationToken cancellationToken = new CancellationToken()
        )
        {
            List<IDomainEventEntity> domainEventEntities =
                DbContextInterceptorHelpers.GetChangedDomainEventEntitiesWithDomainEvents(
                    eventData.Context,
                    true
                );
            await DbContextInterceptorHelpers.ExecuteDomainEvents(
                eventData.Context,
                domainEventEntities,
                _mediator,
                _logger
            );
            await base.TransactionCommittedAsync(transaction, eventData, cancellationToken);
        }
    }
}
