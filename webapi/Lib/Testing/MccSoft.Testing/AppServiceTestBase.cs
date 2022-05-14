using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MccSoft.IntegreSql.EF;
using MccSoft.IntegreSql.EF.DatabaseInitialization;
using MccSoft.LowLevelPrimitives;
using MccSoft.NpgSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Debug;
using Moq;

namespace MccSoft.Testing
{
    /// <summary>
    /// The base class for application service tests.
    /// </summary>
    /// <remarks>
    /// This class serves to enforce a specific way of accessing the DB:
    /// * the service works with it's own instance of DbContext,
    ///   who lives at least as long as the service instance itself,
    /// * arrange- and assert-blocks of tests use a separate short-lived DbContext provided by
    ///   the helper method <see cref="WithDbContext"/>.
    ///
    /// This approach allows tests to check whether SaveChanges was called in the service method
    /// (the state of objects loaded in a separate DbContext will be incorrect, if SaveChanges is
    /// forgotten).
    /// </remarks>
    public class AppServiceTestBase<TService, TDbContext> : TestBase<TDbContext>, IDisposable
        where TDbContext : DbContext, ITransactionFactory
    {
        protected readonly ILoggerFactory LoggerFactory = new LoggerFactory(
            new[] { new DebugLoggerProvider() }
        );

        private readonly Func<
            DbContextOptions<TDbContext>,
            IUserAccessor,
            TDbContext
        > _dbContextFactory;

        private readonly DbContextOptionsBuilder<TDbContext> _builder;
        private TDbContext _dbContext;

        protected readonly Mock<IUserAccessor> _userAccessorMock;

        private readonly TestDatabaseType _databaseType;
        private readonly IDatabaseInitializer _databaseInitializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServiceTestBase{TService,TDbContext}" />
        /// class with the specified DbContext factory.
        /// </summary>
        /// <param name="databaseType">Type of database to use in tests</param>
        /// <param name="dbContextFactory">A function that creates a DbContext.</param>
        /// <param name="basicDatabaseSeedingOptions">Additional database seeding (beside EnsureCreated)</param>
        protected AppServiceTestBase(
            TestDatabaseType databaseType,
            Func<DbContextOptions<TDbContext>, IUserAccessor, TDbContext> dbContextFactory,
            DatabaseSeedingOptions<TDbContext> basicDatabaseSeedingOptions = null
        )
        {
            _databaseType = databaseType;
            _dbContextFactory = dbContextFactory;

            _databaseInitializer = databaseType switch
            {
                TestDatabaseType.None => null,
                TestDatabaseType.Postgres
                  => new NpgsqlDatabaseInitializer(
                      connectionStringOverride: new() { Host = "localhost", Port = 5434, }
                  ),
                TestDatabaseType.Sqlite => new SqliteDatabaseInitializer(),
                _ => throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null)
            };

            string connectionString = _databaseInitializer?.CreateDatabaseGetConnectionStringSync(
                basicDatabaseSeedingOptions
            );

            _userAccessorMock = new Mock<IUserAccessor>();
            _userAccessorMock.Setup(x => x.GetUserId()).Returns("123");
            _userAccessorMock.Setup(x => x.IsHttpContextAvailable).Returns(true);

            if (databaseType != TestDatabaseType.None)
            {
                // Its ok to call virtual methods because its just init the builder and doesn't use members.
                // ReSharper disable VirtualMemberCallInConstructor
                _builder = GetBuilder(connectionString ?? "");

                EnsureDbCreated();
                // ReSharper restore VirtualMemberCallInConstructor
            }
        }

        public void Dispose()
        {
            DisposeImpl();
        }

        /// <summary>
        /// Makes sure that the DB is created.
        /// </summary>
        protected virtual void EnsureDbCreated()
        {
            using TDbContext context = CreateDbContext();
            context.Database.EnsureCreated();
        }

        /// <summary>
        /// Returns the DbContextOptionsBuilder
        /// </summary>
        /// <param name="connectionString"></param>
        public virtual DbContextOptionsBuilder<TDbContext> GetBuilder(string connectionString)
        {
            var builder = new DbContextOptionsBuilder<TDbContext>();
            _databaseInitializer.UseProvider(builder, connectionString);

            builder
                .UseLoggerFactory(LoggerFactory)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
            return builder;
        }

        /// <summary>
        /// Gets the long-living DbContext that is used to initialize the service under test.
        /// Should be used in alternative implementations of <see cref="InitializeService" />.
        /// </summary>
        /// <returns>The long-living DbContext instance.</returns>
        protected TDbContext GetLongLivingDbContext()
        {
            return _dbContext ??= CreateDbContext();
        }

        /// <summary>
        /// Provides access to a PostgresRetryHelper and DbContext
        /// that should be used to initialize the service being tested.
        /// </summary>
        /// <param name="action">The action that creates the service.</param>
        protected TService InitializeService(
            Func<PostgresRetryHelper<TDbContext, TService>, TDbContext, TService> action
        )
        {
            if (_databaseType == TestDatabaseType.None)
                return action(null, null);
            return action(CreatePostgresRetryHelper<TService>(), GetLongLivingDbContext());
        }

        /// <summary>
        /// Creates PostgresRetryHelper for any service type
        /// </summary>
        protected PostgresRetryHelper<
            TDbContext,
            TAnyService
        > CreatePostgresRetryHelper<TAnyService>()
        {
            var loggerFactory = new NullLoggerFactory();
            return new PostgresRetryHelper<TDbContext, TAnyService>(
                CreateDbContext(),
                CreateDbContext,
                loggerFactory,
                new TransactionLogger<TAnyService>(SetupLogger<TAnyService>())
            );
        }

        /// <summary>
        /// Override this method to dispose of expensive resources created in descendants
        /// of this class. Always call the base method.
        /// </summary>
        protected virtual void DisposeImpl()
        {
            _dbContext?.Dispose();
            _databaseInitializer?.RemoveDatabase(CreateDbContext().Database.GetConnectionString());
            LoggerFactory.Dispose();
        }

        /// <summary>
        /// Creates a new instance of <typeparamref name="TDbContext"/>.
        /// Should be used in alternative implementations of <see cref="InitializeService" />.
        /// </summary>
        /// <returns>A new DbContext instance.</returns>
        protected override TDbContext CreateDbContext()
        {
            return _dbContextFactory(_builder.Options, _userAccessorMock.Object);
        }

        /// <summary>
        /// Gets the list of messages logged by <see cref="Sut"/>.
        /// For proper registering of log messages, SUT must be configured
        /// with the logger returned from <see cref="SetupLogger"/>.
        /// </summary>
        public List<LoggedMessage> LoggedMessages { get; } = new List<LoggedMessage>();

        /// <summary>
        /// Gets the service being tested (System Under Test).
        /// </summary>
        protected TService Sut { get; set; }

        /// <summary>
        /// Gets a mocked logger that appends messages to <see cref="LoggedMessages"/>.
        /// </summary>
        public ILogger<TAnyService> SetupLogger<TAnyService>()
        {
            var loggerMock = new Mock<ILogger<TAnyService>>();
            loggerMock
                .Setup(
                    logger =>
                        logger.Log(
                            It.IsAny<LogLevel>(),
                            It.IsAny<EventId>(),
                            It.IsAny<It.IsAnyType>(),
                            It.IsAny<Exception>(),
                            // https://github.com/dotnet/extensions/issues/1319
                            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                        )
                )
                .Callback(
                    (LogLevel level, EventId eId, object state, Exception ex, object formatter) =>
                    {
                        LoggedMessages.Add(
                            new LoggedMessage
                            {
                                Level = level,
                                LogValues = state as IReadOnlyList<KeyValuePair<string, object>>,
                                Message = state.ToString()
                            }
                        );
                    }
                );
            return loggerMock.Object;
        }
    }
}
