namespace MccSoft.DomainHelpers.DomainEvents
{
    public interface IDomainEventWithIntegerId : IDomainEvent
    {
        /// <summary>
        /// Id will be assigned by EntityFramework when entity is saved
        /// </summary>
        public int Id { get; set; }
    }
}
