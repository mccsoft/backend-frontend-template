using System.Collections.Generic;

namespace MccSoft.DomainHelpers.DomainEvents
{
    public interface IDomainEventEntity
    {
        IReadOnlyList<IDomainEvent> DomainEvents { get; }
        void AddEvent(IDomainEvent domainEvent, bool removeEventsOfSameType = false);
        void ClearDomainEvents();
    }
}
