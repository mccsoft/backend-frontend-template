using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

namespace MccSoft.Testing.SqliteUtils.EFExtensions
{
    public sealed class CustomSqliteMethodCallTranslatorPlugin : SqliteMethodCallTranslatorProvider
    {
        public CustomSqliteMethodCallTranslatorPlugin(
            RelationalMethodCallTranslatorProviderDependencies dependencies
        ) : base(dependencies)
        {
            ISqlExpressionFactory expressionFactory = dependencies.SqlExpressionFactory;
            AddTranslators(
                new List<IMethodCallTranslator>
                {
                    new PostgresToSqliteMethodCallTranslator(expressionFactory)
                }
            );
        }
    }
}
