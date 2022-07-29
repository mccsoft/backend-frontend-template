using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MccSoft.NpgSql
{
    public static class Extensions
    {
        /// <summary>
        /// Postgresql specific action.
        /// If you are using context.Database.Migrate() to create your enums,
        /// you need to instruct Npgsql to reload all types after applying your migrations
        /// For more info refer to: https://www.npgsql.org/efcore/mapping/enum.html?tabs=tabid-1#creating-your-database-enum
        /// </summary>
        public static void ReloadTypesForEnumSupport(this DbContext context)
        {
            var conn = (NpgsqlConnection)context.Database.GetDbConnection();
            conn.Open();
            conn.ReloadTypes();
        }
    }
}
