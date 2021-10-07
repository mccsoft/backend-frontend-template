using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MccSoft.Testing.SqliteUtils
{
    /// <summary>
    /// SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
    /// here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
    /// To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
    /// use the DateTimeOffsetToBinaryConverter
    /// Based on: https://blog.dangl.me/archive/handling-datetimeoffset-in-sqlite-with-entity-framework-core/
    /// This only supports millisecond precision, but should be sufficient for most use cases.
    /// </summary>
    public class ModelCustomizerWithPatchedDateTimeOffset : ModelCustomizer
    {
        public ModelCustomizerWithPatchedDateTimeOffset(
            ModelCustomizerDependencies dependencies
        ) : base(dependencies) { }

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);

            UseValueConverterForType<DateTimeOffset>(
                modelBuilder,
                new DateTimeOffsetToStringConverter()
            );
            UseValueConverterForType<DateTimeOffset?>(
                modelBuilder,
                new DateTimeOffsetToStringConverter()
            );
        }

        private ModelBuilder UseValueConverterForType<T>(
            ModelBuilder modelBuilder,
            ValueConverter converter
        ) {
            return UseValueConverterForType(modelBuilder, typeof(T), converter);
        }

        private ModelBuilder UseValueConverterForType(
            ModelBuilder modelBuilder,
            Type type,
            ValueConverter converter
        ) {
#pragma warning disable EF1001 // Internal EF Core API usage.
            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                List<PropertyInfo> properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == type)
                    .ToList();

                var eType = (Microsoft.EntityFrameworkCore.Metadata.Internal.EntityType)entityType;
                Microsoft.EntityFrameworkCore.Metadata.Internal.InternalEntityTypeBuilder builder =
                    eType.Builder;

                foreach (PropertyInfo property in properties)
                {
                    builder.Property(type, property.Name, ConfigurationSource.Explicit)
                        .HasConversion(converter, ConfigurationSource.Explicit);
                }
            }
#pragma warning restore EF1001 // Internal EF Core API usage.

            return modelBuilder;
        }
    }
}
