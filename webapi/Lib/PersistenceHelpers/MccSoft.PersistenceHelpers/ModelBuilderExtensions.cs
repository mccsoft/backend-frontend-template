using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MccSoft.PersistenceHelpers;

public static class ModelBuilderExtensions
{
    // Source: https://andrewlock.net/using-strongly-typed-entity-ids-to-avoid-primitive-obsession-part-3/
    /// <summary>
    /// Configures all properties that have an appropriate type
    /// so that a property value is converted to and from the database using the given ValueConverter.
    /// </summary>
    /// <param name="modelBuilder"></param>
    /// <param name="converter">The converter to use</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public static ModelBuilder UseValueConverter(
        this ModelBuilder modelBuilder,
        ValueConverter converter
    )
    {
        var type = converter.ModelClrType;

        // For all entities in the data model
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Find the properties that are of the type to convert
            var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == type);

            foreach (var property in properties)
            {
                // Use the value converter for the property
                modelBuilder
                    .Entity(entityType.ClrType)
                    .Property(property.Name)
                    .HasConversion(converter);
            }
        }

        return modelBuilder;
    }

    /// <summary>
    /// Configures all properties that have an appropriate type
    /// so that a property value is compared using the given ValueComparer.
    /// </summary>
    /// <param name="modelBuilder"></param>
    /// <param name="comparer">The comparer to use</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public static ModelBuilder UseValueComparer(
        this ModelBuilder modelBuilder,
        ValueComparer comparer
    )
    {
        var type = comparer.Type;

        // For all entities in the data model
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Find the properties that are of the type to compare
            var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == type);

            foreach (var property in properties)
            {
                // Use the value comparer for the property
                modelBuilder
                    .Entity(entityType.ClrType)
                    .Property(property.Name)
                    .Metadata.SetValueComparer(comparer);
            }
        }

        return modelBuilder;
    }
}
