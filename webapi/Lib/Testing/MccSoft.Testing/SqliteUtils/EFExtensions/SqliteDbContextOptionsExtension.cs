using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MccSoft.Testing.SqliteUtils.EFExtensions
{
    public class SqliteDbContextOptionsExtension : IDbContextOptionsExtension
    {
        private DbContextOptionsExtensionInfo _info;

        public void Validate(IDbContextOptions options) { }

        public DbContextOptionsExtensionInfo Info
        {
            get { return this._info ??= new MyDbContextOptionsExtensionInfo(this); }
        }

        void IDbContextOptionsExtension.ApplyServices(IServiceCollection services)
        {
            services.TryAddScoped<
                IMethodCallTranslatorProvider,
                CustomSqliteMethodCallTranslatorPlugin
            >();
        }

        private sealed class MyDbContextOptionsExtensionInfo : DbContextOptionsExtensionInfo
        {
            public MyDbContextOptionsExtensionInfo(IDbContextOptionsExtension instance)
                : base(instance) { }

            public override bool IsDatabaseProvider => true;

            public override string LogFragment => "";

            public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            {
                return true;
            }

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) { }

            public override int GetServiceProviderHashCode()
            {
                return 0;
            }
        }
    }
}
