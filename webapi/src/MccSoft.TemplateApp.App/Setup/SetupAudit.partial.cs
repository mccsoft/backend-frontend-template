﻿using System.Data.Common;
using System.Text.Json;
using Audit.Core;
using Audit.EntityFramework;
using MccSoft.Logging;
using MccSoft.TemplateApp.App.Utils;
using MccSoft.TemplateApp.Domain.Audit;
using MccSoft.TemplateApp.Persistence;
using MccSoft.WebApi.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace MccSoft.TemplateApp.App.Setup;

public partial class SetupAudit
{
    private static partial TemplateAppDbContext CreateAuditDbContext(TemplateAppDbContext dbContext)
    {
        DbConnection conn = dbContext.Database.GetDbConnection();
        return new TemplateAppDbContext(
            new DbContextOptionsBuilder<TemplateAppDbContext>().UseNpgsql(conn).Options,
            dbContext.UserAccessor
        );
    }

    private static partial object CreateAuditMessageForSerilog(AuditEvent auditEvent, object arg2)
    {
        var logger = Serilog.Log.ForContext<AuditLogContext>();
        if (auditEvent is AuditEventEntityFramework entityFrameworkEvent)
        {
            foreach (var eventEntry in entityFrameworkEvent.EntityFrameworkEvent.Entries)
            {
                var changes =
                    eventEntry.Changes == null
                        ? null
                        : Audit.Core.Configuration.JsonAdapter.Serialize(
                            eventEntry.Changes.Where(
                                x =>
                                    !(x.NewValue == null && x.OriginalValue == null)
                                    && x.NewValue?.Equals(x.OriginalValue) != true
                            )
                        );
                var primaryKey =
                    eventEntry.PrimaryKey == null
                        ? null
                        : eventEntry.PrimaryKey.Count == 1
                            ? eventEntry.PrimaryKey.Values.First().ToString()
                            : string.Join(", ", eventEntry.PrimaryKey.Values);

                logger.Information(
                    $"{Field.Method}, {Field.Named("Action")}, Table: {Field.Named("Table")}, Id: {Field.Named("Id")}, Changes: {Field.Named("Changes")}, CurrentValues: {Field.Named("CurrentValues")}",
                    "EFAudit",
                    eventEntry.Action,
                    eventEntry.Table,
                    primaryKey,
                    changes,
                    eventEntry.ColumnValues
                );
            }

            return null;
        }
        else
        {
            return auditEvent.ToJson();
        }
    }

    /// <summary>
    /// Used only for creating logger context (Serilog.Log.ForContext<AuditLogContext>())
    /// </summary>
    private class AuditLogContext { }

    private static void SetupSavingToEfCore()
    {
        Audit.Core.Configuration
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
    }

    private static void SetupSavingToSerilog()
    {
        Audit.Core.Configuration
            .Setup()
            .UseSerilog(
                config =>
                    config
                        .LogLevel(Audit.NET.Serilog.LogLevel.Info)
                        .Message(CreateAuditMessageForSerilog)
            );
    }
}
