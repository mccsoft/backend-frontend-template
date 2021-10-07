namespace MccSoft.DomainHelpers.DomainEvents
{
    public interface IDomainEventWithStringId : IDomainEvent
    {
        /// <summary>
        /// Id will be assigned by EntityFramework when entity is saved
        /// </summary>
        public string Id { get; set; }
    }
}
