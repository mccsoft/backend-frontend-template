using System.Data;
using System.Threading.Tasks;
using Audit.EntityFramework;
using MccSoft.LowLevelPrimitives;
using MccSoft.NpgSql;
using MccSoft.PersistenceHelpers;
using MccSoft.TemplateApp.Domain;
using MccSoft.TemplateApp.Domain.Audit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace MccSoft.TemplateApp.Persistence;

public class TemplateAppDbContext
    :
    // DbContext
    IdentityDbContext<User>,
        ITransactionFactory
{
    public IUserAccessor UserAccessor { get; }
    public DbSet<Product> Products { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    public TemplateAppDbContext(
        DbContextOptions<TemplateAppDbContext> options,
        IUserAccessor userAccessor
    ) : base(options)
    // : base(options)
    {
        UserAccessor = userAccessor;
    }

    static TemplateAppDbContext() => NpgsqlConnection.GlobalTypeMapper.MapEnum<ProductType>();

    // when adding enum here don't forget to OnModelCreating as well (see below)

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasPostgresEnum<ProductType>();
        // when adding enum here don't forget to add it to static constructor as well (see above)

        // if you already have some data in the table, which column you'd like to convert to enum
        // you'd need to adjust migration SQL to something like the following
        // migrationBuilder.Sql(
        // @"ALTER TABLE ""Patients"" ALTER COLUMN ""NumberSource"" TYPE number_source using (enum_range(null::number_source))[""NumberSource""::int + 1];"
        //     );
        // For details see https://github.com/mccsoft/backend-frontend-template/wiki/_new#migration-of-existing-data

        builder.SetupQueryFilter<IOwnedEntity>(
            (x) => CurrentOwnerId == null || x.OwnerId == CurrentOwnerId
        );
        builder.AddWebHookEntities(this.GetType());
    }

    public IDbContextTransaction BeginTransaction()
    {
        return Database.BeginTransaction(IsolationLevel.Serializable);
    }

    public Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return Database.BeginTransactionAsync(IsolationLevel.Serializable);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.AddInterceptors(
            new PostProcessEntitiesOnSaveInterceptor<IOwnedEntity, TemplateAppDbContext>(
                (entity, context) =>
                {
                    if (!string.IsNullOrEmpty(context.CurrentOwnerId))
                    {
                        entity.SetOwnerIdUnsafe(context.CurrentOwnerId);
                    }
                }
            )
        );

        optionsBuilder.AddInterceptors(new AuditSaveChangesInterceptor());
    }

    public string? CurrentOwnerId =>
        UserAccessor.IsHttpContextAvailable ? UserAccessor.GetUserId() : null;
}
