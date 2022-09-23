using System;
using MccSoft.DomainHelpers;

namespace MccSoft.TemplateApp.Domain;

public class Product : BaseOwnedEntity
{
    private string _title;
    public int Id { get; set; }
    public ProductType ProductType { get; set; }

    /// <summary>
    /// This is just to show how to get the userId in the Service.
    /// You should NOT add this field to every entity (unless explicitly needed)
    /// </summary>
    public User CreatedByUser { get; set; }
    public string CreatedByUserId { get; set; }

    public DateOnly LastStockUpdatedAt { get; set; }

    public string Title { get; set; }

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
    public static Specification<Product> HasId(int id) => new(nameof(HasId), p => p.Id == id, id);
}
