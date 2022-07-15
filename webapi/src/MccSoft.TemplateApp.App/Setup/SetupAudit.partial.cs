using System.Data.Common;
using MccSoft.TemplateApp.Persistence;
using Microsoft.EntityFrameworkCore;

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
}
