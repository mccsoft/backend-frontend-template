using System.Data.Common;
using System.Text.Json;
using Audit.Core;
using MccSoft.LowLevelPrimitives.Serialization.DateOnlyConverters;
using MccSoft.TemplateApp.App.Settings;
using MccSoft.TemplateApp.App.Utils;
using MccSoft.TemplateApp.Domain;
using MccSoft.TemplateApp.Domain.Audit;
using MccSoft.TemplateApp.Persistence;
using MccSoft.WebApi.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace MccSoft.TemplateApp.App.Setup;

public static partial class SetupAudit
{
    public static IHttpContextAccessor HttpContextAccessor { get; set; }

    public static void ConfigureAudit(IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("Audit");
        services.Configure<AuditSettings>(settings);
        var typedSettings = settings.Get<AuditSettings>();

        Configuration.AuditDisabled = !typedSettings.Enabled;

        Configuration
            .Setup()
            .UseEntityFramework(
                config =>
                    config
                        .UseDbContext(ev =>
                        {
                            // https://github.com/thepirat000/Audit.NET/issues/451
                            // To support transactions rollback (e.g. DbRetryHelper) we need to use separate context for Audit entries
                            // (otherwise there's an infinite cycle inside Audit.Net when transaction is rolled back and next transaction is committed).
                            // But we also need to use the same transaction for audit logs as in the main context, to not save Audit logs from rolled back transactions.
                            // So we create DBContext manually
                            TemplateAppDbContext dbContext = (TemplateAppDbContext)
                                ev.EntityFrameworkEvent.GetDbContext();
                            DatabaseFacade db = dbContext.Database;
                            IDbContextTransaction tran = db.CurrentTransaction;
                            TemplateAppDbContext auditContext = CreateAuditDbContext(dbContext);
                            if (tran != null)
                            {
                                auditContext.Database.UseTransaction(tran.GetDbTransaction());
                            }

                            return auditContext;
                        })
                        .AuditTypeMapper(x => x == typeof(AuditLog) ? null : typeof(AuditLog))
                        .AuditEntityAction<AuditLog>(
                            (ev, entry, auditLog) =>
                            {
                                auditLog.UserId =
                                    HttpContextAccessor?.HttpContext?.User?.Identity?.GetUserIdOrNull();
                                auditLog.ChangeDate = DateTime.UtcNow;
                                auditLog.EntityType = entry.Name;
                                auditLog.Action = entry.Action;
                                auditLog.FullKey = JsonSerializer.SerializeToDocument(
                                    entry.PrimaryKey,
                                    SystemTextJsonSerializerSetup.GlobalSerializationOptions
                                );
                                auditLog.Key = entry.PrimaryKey.Values.FirstOrDefault()?.ToString();
                                auditLog.Change = JsonSerializer.SerializeToDocument(
                                    entry.Changes?.ToDictionary(x => x.ColumnName, x => x.NewValue),
                                    SystemTextJsonSerializerSetup.GlobalSerializationOptions
                                );
                                auditLog.Actual = JsonSerializer.SerializeToDocument(
                                    entry.ColumnValues,
                                    SystemTextJsonSerializerSetup.GlobalSerializationOptions
                                );
                                Dictionary<string, object> old = entry.ColumnValues.ToDictionary(
                                    x => x.Key,
                                    x => x.Value
                                );
                                entry.Changes?.ForEach(x => old[x.ColumnName] = x.OriginalValue);
                                auditLog.Old =
                                    entry.Changes == null
                                        ? null
                                        : JsonSerializer.SerializeToDocument(
                                            old,
                                            SystemTextJsonSerializerSetup.GlobalSerializationOptions
                                        );
                            }
                        )
                        .IgnoreMatchedProperties(true)
            );

        Audit.EntityFramework.Configuration
            .Setup()
            .ForContext<TemplateAppDbContext>(
                config => config.ForEntity<User>(_ => _.Ignore(user => user.PasswordHash))
            )
            .UseOptOut();
    }

    private static partial TemplateAppDbContext CreateAuditDbContext(
        TemplateAppDbContext dbContext
    );

    public static void UseAudit(WebApplication app)
    {
        HttpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
    }
}
