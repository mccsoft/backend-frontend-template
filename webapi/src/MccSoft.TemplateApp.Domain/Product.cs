using System;
using MccSoft.DomainHelpers;
using MccSoft.DomainHelpers.DomainEvents.Events;
using Microsoft.Extensions.Logging;

namespace MccSoft.TemplateApp.Domain
{
    public class Product : BaseEntity
    {
        private string _title;
        public int Id { get; set; }
        public ProductType ProductType { get; set; }

        public DateOnly LastStockUpdatedAt { get; set; }

        public string Title
        {
            get => _title;
            set
            {
                if (_title == value)
                    return;

                AddEvent(LogDomainEvent.Info("Title changed to {title}", value));
                _title = value;
            }
        }

        /// <summary>
        /// Needed for Entity Framework, keep empty.
        /// </summary>
        protected Product() { }

        /// <summary>
        /// Constructor to initialize User entity.
        /// </summary>
        public Product(string title)
        {
            Title = title;
        }

        /// <summary>
        /// Creates a specification that is satisfied by a Product having the specified id.
        /// </summary>
        /// <param name="id">The user id.</param>
        /// <returns>The created specification.</returns>
        public static Specification<Product> HasId(int id) =>
            new(nameof(HasId), p => p.Id == id, id);
    }
}
