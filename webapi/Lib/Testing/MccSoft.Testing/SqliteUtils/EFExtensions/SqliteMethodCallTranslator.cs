using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace MccSoft.Testing.SqliteUtils.EFExtensions
{
    public class PostgresToSqliteMethodCallTranslator : IMethodCallTranslator
    {
        private readonly ISqlExpressionFactory _expressionFactory;

        private static readonly MethodInfo iLikeMethod =
            typeof(NpgsqlDbFunctionsExtensions).GetMethod(
                nameof(NpgsqlDbFunctionsExtensions.ILike),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) }
            );

        public PostgresToSqliteMethodCallTranslator(ISqlExpressionFactory expressionFactory)
        {
            _expressionFactory = expressionFactory;
        }

        public SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger
        )
        {
            if (method == iLikeMethod)
            {
                var args = new List<SqlExpression> { arguments[1], arguments[2] }; // cut the first parameter (DBFunctions) from extension function

                return _expressionFactory.Like(args[0], args[1]);
            }

            return null;
        }
    }
}
