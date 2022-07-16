using System.Collections.Generic;
using MccSoft.DomainHelpers.DomainEvents;

namespace MccSoft.TemplateApp.Domain
{
    public partial class BaseEntity : IDomainEventEntity
    {
        #region Domain Events

        private List<IDomainEvent> _domainEvents;
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

        public void AddEvent(IDomainEvent domainEvent, bool removeEventsOfSameType = false)
        {
            _domainEvents ??= new List<IDomainEvent>();
            if (removeEventsOfSameType)
            {
                _domainEvents.RemoveAll(x => x.GetType() == domainEvent.GetType());
            }

            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
        #endregion
    }
}
