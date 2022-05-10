using System.Globalization;
using System.Threading;
using Hangfire;
using MccSoft.NpgSql;
using MccSoft.TemplateApp.Domain;
using MccSoft.TemplateApp.Persistence;
using MccSoft.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace MccSoft.TemplateApp.App.Tests
{
    /// <summary>
    /// The base class for application service test classes.
    /// </summary>
    /// <typeparam name="TService">The type of the service under test.</typeparam>
    public class AppServiceTestBase<TService> : AppServiceTestBase<TService, TemplateAppDbContext>
    {
        protected Mock<IBackgroundJobClient> _backgroundJobClient;
        private User _defaultUser;

        public AppServiceTestBase()
            : base((options, userAccessor) => new TemplateAppDbContext(options, userAccessor))
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            WithDbContext(
                db =>
                {
                    db.Users.Add(_defaultUser = new User("default@test.test"));
                    db.SaveChanges();
                }
            );
            _userAccessorMock.Setup(x => x.GetUserId()).Returns(_defaultUser.Id);
            PostgresSerialization.AdjustDateOnlySerialization();
            Audit.Core.Configuration.AuditDisabled = true;
        }

        protected ServiceCollection CreateServiceCollection(TemplateAppDbContext db)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(db);

            serviceCollection.AddTransient(typeof(ILogger<>), typeof(NullLogger<>));

            _backgroundJobClient = new Mock<IBackgroundJobClient>();
            serviceCollection.AddSingleton(_backgroundJobClient.Object);

            return serviceCollection;
        }
    }
}
