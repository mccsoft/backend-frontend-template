﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using MccSoft.IntegreSql.EF;
using MccSoft.IntegreSql.EF.DatabaseInitialization;
using MccSoft.LowLevelPrimitives;
using MccSoft.PersistenceHelpers;
using MccSoft.PersistenceHelpers.DomainEvents;
using MccSoft.Testing.AspNet;
using MccSoft.Testing.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NeinLinq;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Xunit.Abstractions;

namespace MccSoft.Testing;

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
public abstract class TestBase<TDbContext>
    where TDbContext : DbContext
{
    protected string ConnectionString { get; private set; }
    public ITestOutputHelper OutputHelper { get; set; }
    protected TDbContext _dbContext;
    protected Mock<IBackgroundJobClient> _backgroundJobClient;
    protected IServiceProvider _serviceProvider;
    protected Mock<IUserAccessor> _userAccessorMock;

    /// <summary>
    /// The entity factory.
    /// The non-conventional name is chosen to read as an English article:
    /// a.DomainClass(...args...)
    /// For classes starting with a vowel use <see cref="an"/>.
    /// </summary>
    public static MotherFactory a = null;

    /// <summary>
    /// The entity factory.
    /// The non-conventional name is chosen to read as an English article:
    /// an.Entity(...args...)
    /// For classes starting with a consonant use <see cref="a"/>.
    /// </summary>
    public static MotherFactory an = null;

    protected readonly DatabaseType? _databaseType;
    protected readonly IDatabaseInitializer _databaseInitializer;
    protected Mock<IWebHostEnvironment> _webHostEnvironment;

    protected TestBase(
        ITestOutputHelper outputHelper,
        DatabaseType? databaseType,
        Action<NpgsqlDbContextOptionsBuilder> npgsqlOptionsAction = null
    )
    {
        _databaseType = databaseType;
        OutputHelper = outputHelper;
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

        _userAccessorMock = new Mock<IUserAccessor>();

        _databaseInitializer = databaseType switch
        {
            null => null,
            DatabaseType.Postgres
                => new NpgsqlDatabaseInitializer(
                    connectionStringOverride: new() { Host = "localhost", Port = 5434, },
                    npgsqlOptionsAction: npgsqlOptionsAction
                ),
            DatabaseType.Sqlite => new SqliteDatabaseInitializer(),
            _ => throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null)
        };

        if (databaseType != null)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            InitializeDatabase(SeedDatabase());
        }
    }

    /// <summary>
    /// Insert some data that you want to be available in every test.
    ///
    /// Note, that this method is executed only once, when template database is initially created.
    /// !!IT IS NOT CALLED IN EACH TEST!!!
    /// (though, created data is available in each test by backing up
    /// and restoring a DB from template for each test)
    /// </summary>
    protected abstract DatabaseSeedingOptions<TDbContext> SeedDatabase();

    #region WithDbContext

    protected virtual TDbContext CreateDbContext()
    {
        return _serviceProvider != null
            ? CreateDbContext(_serviceProvider)
            : CreateService<TDbContext>();
    }

    /// <summary>
    /// Creates a NEW DbContext.
    /// Resolving it from ServiceProvider is not enough, because we will get the same DbContext every time.
    /// </summary>
    protected abstract TDbContext CreateDbContext(IServiceProvider serviceProvider);

    /// <summary>
    /// !!!!!!!!!!!!!
    /// Consider using <see cref="WithDbContext{T}"/> (async) instead!
    /// Using synchronous version could make tests hang when running out of threads in a pool.
    /// !!!!!!!!!!!!
    /// Provides access to a short-lived DbContext, independent from the service DbContext,
    /// but sharing the same DB.
    /// Should be used to prepare data and make assertions.
    /// </summary>
    /// <param name="action">The action to execute with the DbContext.</param>
    protected void WithDbContextSync(Action<TDbContext> action)
    {
        using var db = CreateDbContext();
        action(db);
    }

    /// <summary>
    /// !!!!!!!!!!!!!
    /// Consider using <see cref="WithDbContext{T}"/> (async) instead!
    /// Using synchronous version could make tests hang when running out of threads in a pool.
    /// !!!!!!!!!!!!
    /// Provides access to a short-lived DbContext, independent from the service DbContext,
    /// but sharing the same DB.
    /// Should be used to prepare data and make assertions.
    /// </summary>
    /// <param name="action">The action to execute with the DbContext.</param>
    protected T WithDbContextSync<T>(Func<TDbContext, T> action)
    {
        using var db = CreateDbContext();
        var result = action(db);

        return result;
    }

    /// <summary>
    /// Provides access to a short-lived DbContext, independent from the service DbContext,
    /// but sharing the same DB.
    /// Should be used to prepare data and make assertions.
    /// </summary>
    /// <param name="action">The action to execute with the DbContext.</param>
    protected async Task<T> WithDbContext<T>(Func<TDbContext, Task<T>> action)
    {
        await using var db = CreateDbContext();
        return await action(db);
    }

    /// <summary>
    /// Provides access to a short-lived DbContext, independent from the service DbContext,
    /// but sharing the same DB.
    /// Should be used to prepare data and make assertions.
    /// </summary>
    /// <param name="action">The action to execute with the DbContext.</param>
    protected async Task WithDbContext(Func<TDbContext, Task> action)
    {
        await using var db = CreateDbContext();
        await action(db);
    }

    #endregion

    #region InitializeService

    /// <summary>
    /// We need to insert the logger in App Tests
    /// (because we don't spin up the whole TestServer there, so there's no built-in logging to use)
    /// But we don't need to insert it in Component Tests, because there's a built-in logger already
    /// which could be controlled via standard appsettings.json
    /// </summary>
    public virtual bool InsertLoggerInEf => false;

    /// <summary>
    /// Returns the DbContextOptionsBuilder
    /// !!! ServiceProvider will be null when we are seeding the database !!!
    /// Don't forget to check it for null!
    /// </summary>
    protected virtual void ConfigureDatabaseOptions(
        IServiceProvider? serviceProvider,
        DbContextOptionsBuilder builder,
        string connectionString
    )
    {
        _databaseInitializer.UseProvider(builder, connectionString);
        builder
            .WithLambdaInjection()
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .UseOpenIddict();

        if (serviceProvider != null)
            builder.AddDomainEventsInterceptors(serviceProvider);

        if (InsertLoggerInEf)
        {
            // this is needed in App Tests, but not in Component Tests.
            // see comments to InsertLoggerInEf for details
            builder.UseLoggerFactory(LoggerFactory.Create(ConfigureXunitLogger()));
        }
    }

    /// <summary>
    /// Gets the long-living DbContext that is used to initialize the service under test.
    /// </summary>
    /// <returns>The long-living DbContext instance.</returns>
    protected virtual TDbContext GetLongLivingDbContext()
    {
        return _dbContext ??= CreateDbContext();
    }

    /// <summary>
    /// Provides access to a DbRetryHelper and DbContext
    /// that should be used to initialize the service being tested.
    /// </summary>
    /// <param name="action">The action that creates the service.</param>
    protected virtual TService CreateService<TService>(
        Func<DbRetryHelper<TDbContext, TService>, TDbContext, TService> action
    )
    {
        if (_databaseType == null)
            return action(null, null);
        return action(CreateDbRetryHelper<TService>(), GetLongLivingDbContext());
    }

    /// <summary>
    /// Creates DbRetryHelper for any service type
    /// </summary>
    protected DbRetryHelper<TDbContext, TService> CreateDbRetryHelper<TService>()
    {
        return CreateService<DbRetryHelper<TDbContext, TService>>();
    }

    /// <summary>
    /// Creates the Service Under Test.
    /// If you want to override/mock some dependencies, please do it in the <paramref name="configureRegistrations"/>
    /// </summary>
    /// <param name="configureRegistrations">provides ability to register mocks for some services</param>
    protected virtual TService CreateService<TService>(
        Action<IServiceCollection> configureRegistrations = null
    )
        where TService : class
    {
        return CreateService<TService>(configureRegistrations, out _);
    }

    /// <summary>
    /// Creates the Service Under Test.
    /// Returns <paramref name="serviceProvider" /> as `out` parameter, so you could resolve other services from it (in the same Scope as SUT).
    /// </summary>
    protected virtual TService CreateService<TService>(out IServiceProvider serviceProvider)
        where TService : class
    {
        return CreateService<TService>(null, out serviceProvider);
    }

    /// <summary>
    /// Creates the Service Under Test.
    /// If you want to override/mock some dependencies, please do it in the <paramref name="configureRegistrations"/>
    /// Returns <paramref name="serviceProvider"/> as `out` parameter, so you could resolve other services from it (in the same Scope as SUT).
    /// </summary>
    /// <param name="configureRegistrations">provides ability to register mocks for some services</param>
    /// <param name="serviceProvider">
    /// Out parameter.
    /// Service Provider the service is resolved from.
    /// You could resolve other services from it.
    /// </param>
    protected virtual TService CreateService<TService>(
        Action<IServiceCollection> configureRegistrations,
        out IServiceProvider serviceProvider
    )
        where TService : class
    {
        serviceProvider = CreateServiceProvider(configureRegistrations);
        var scope = serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TService>();
    }

    protected virtual IServiceProvider CreateServiceProvider(
        Action<IServiceCollection> configureRegistrations
    )
    {
        var serviceCollection = CreateServiceCollection();
        configureRegistrations?.Invoke(serviceCollection);

        _serviceProvider = serviceCollection.BuildServiceProvider();
        return _serviceProvider;
    }

    /// <summary>
    /// Creates a service collection that you could resolve your System-Under-Test from.
    /// </summary>
    protected virtual ServiceCollection CreateServiceCollection()
    {
        var serviceCollection = new ServiceCollection();

        var (configurationBuilder, environment) = SetupEnvironment();
        var configuration = configurationBuilder.Build();

        /*
         * Please, register your services in RegisterServices method, not here!
         * This place should remain project-independent (it's a Library that's used in many projects)
         */
        RegisterServices(serviceCollection, configuration, environment);

        RegisterBaseTypes(serviceCollection, configuration);

        /*
         * DO NOT register your project-specific services here!
         * Register your app-specific services in RegisterServices method
         */

        return serviceCollection;
    }

    private void RegisterBaseTypes(
        ServiceCollection serviceCollection,
        IConfiguration configuration
    )
    {
        /*
         * DO NOT register your project-specific services here!
         * Register your app-specific services in RegisterServices method
         */
        RegisterDbContext(serviceCollection, ConnectionString);

        serviceCollection
            .AddSingleton<IStringLocalizer>(new DummyStringLocalizer())
            .AddSingleton(typeof(IStringLocalizer<>), typeof(DummyStringLocalizer<>));

        _userAccessorMock.Setup(x => x.IsHttpContextAvailable).Returns(true);
        serviceCollection.AddSingleton(_userAccessorMock.Object);

        serviceCollection.AddSingleton(
            (_backgroundJobClient = HangfireMock.CreateHangfireMock(() => _serviceProvider)).Object
        );
        serviceCollection.AddSingleton(
            (_webHostEnvironment = new Mock<IWebHostEnvironment>()).Object
        );

        serviceCollection.AddLogging(ConfigureXunitLogger());

        serviceCollection.AddSingleton(configuration);

        /*
         * DO NOT register your project-specific services here!
         * Register your app-specific services in RegisterServices method
         */
    }

    private static Assembly[] _assemblies = AppDomain.CurrentDomain.GetAssemblies();

    protected virtual void RegisterDbContext(
        IServiceCollection serviceCollection,
        string connectionString
    )
    {
        serviceCollection.AddDomainEventsWithMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(_assemblies);
        });
        serviceCollection
            .AddDbContext<TDbContext>(
                (serviceProvider, optionsBuilder) =>
                {
                    ConfigureDatabaseOptions(serviceProvider, optionsBuilder, connectionString);
                },
                contextLifetime: ServiceLifetime.Scoped,
                optionsLifetime: ServiceLifetime.Singleton
            )
            .AddScoped<Func<TDbContext>>((provider) => () => CreateDbContext(provider))
            .RegisterRetryHelper();
    }

    private Action<ILoggingBuilder> ConfigureXunitLogger()
    {
        return loggingBuilder => loggingBuilder.ClearProviders().AddXUnit(OutputHelper);
    }

    protected virtual (ConfigurationBuilder, IWebHostEnvironment) SetupEnvironment()
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder
            .AddJsonFile("appsettings.json", true)
            .AddJsonFile("appsettings.Test.json", true)
            .AddInMemoryCollection(new Dictionary<string, string>() { });
        var environment = new Mock<IWebHostEnvironment>().SetupAllProperties();
        environment.Object.EnvironmentName = "Test";
        return (configurationBuilder, environment.Object);
    }

    /// <summary>
    /// Here you could register your app-specific services (override this method and register!)
    /// </summary>
    protected abstract void RegisterServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment
    );

    #endregion

    #region Initialize database

    protected virtual void InitializeDatabase(
        DatabaseSeedingOptions<TDbContext> seedingOptions = null
    )
    {
        ConnectionString = _databaseInitializer?.CreateDatabaseGetConnectionStringSync(
            seedingOptions
        );
        OutputHelper.WriteLine($"Connection string: {ConnectionString}");
    }

    #endregion

    #region Dispose

    public void Dispose()
    {
        DisposeImpl();
    }

    /// <summary>
    /// Override this method to dispose of expensive resources created in descendants
    /// of this class. Always call the base method.
    /// </summary>
    protected virtual void DisposeImpl()
    {
        _dbContext?.Dispose();
        _databaseInitializer?.RemoveDatabase(ConnectionString);
    }

    #endregion
}
