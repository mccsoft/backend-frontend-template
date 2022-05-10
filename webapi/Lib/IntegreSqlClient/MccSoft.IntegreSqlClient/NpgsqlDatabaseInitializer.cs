using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using MccSoft.IntegreSqlClient.DbSetUp;
using MccSoft.IntegreSqlClient.Dto;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MccSoft.IntegreSqlClient;

/// <summary>
/// Class that initializes the template database and returns connection string to a fresh instance every time.
/// </summary>
public class NpgsqlDatabaseInitializer : BaseDatabaseInitializer
{
    private readonly IntegreSqlClient _integreSqlClient;

    /// <summary>
    /// When <see cref="NpgsqlDatabaseInitializer.GetConnectionString"/> is called, IntegreSQL returns a connection string it uses to connect to PostgreSQL.
    /// These settings might differ from what you want to use
    /// (e.g. because IntegreSQL uses internal docker hostname/port of Postgres, and your tests are not running in docker).
    /// So you could override some connection string by defining <see cref="ConnectionStringOverride"/>
    /// and setting some properties to non-null values.
    /// </summary>
    public static ConnectionStringOverride ConnectionStringOverride { get; set; } = new();

    /// <summary>
    /// If set to true, we will MD5 the databaseHash that you provide to <see cref="GetConnectionString"/> before passing it to IntegreSQL.
    /// If false, we will pass databaseHash as is.
    /// Note, that if `false` is used, there's a 30 symbols length limit for databaseHash.
    /// </summary>
    public static bool UseMd5Hash = true;

    private record ConnectionStringInfo(string Hash, int Id);

    /// <summary>
    /// Allows to get the Id of database using connection string.
    /// Key - connection string, value - IntegreSQL identifiers of that database.
    /// </summary>
    private static ConcurrentDictionary<string, ConnectionStringInfo> ConnectionStringInfos = new();

    /// <summary>
    /// Constructs NpgsqlDatabaseInitializer
    /// </summary>
    /// <param name="integreSqlUri">URI of IntegreSQL. http://localhost:5000/api/v1/ is used by default</param>
    public NpgsqlDatabaseInitializer(Uri integreSqlUri = null)
    {
        integreSqlUri ??= new Uri("http://localhost:5000/api/v1/");
        _integreSqlClient = new IntegreSqlClient(integreSqlUri);
    }

    /// <summary>
    /// Stores the database initialization tasks.
    /// This is to prevent two parallel initializations of database with the same hash
    /// (if 2 tests starts in parallel).
    /// Key of dictionary is database hash, value - a Task which completes when database initialization is complete.
    /// </summary>
    private static readonly LazyConcurrentDictionary<string, Task> InitializationTasks = new();

    /// <summary>
    /// Returns a PostgreSQL connection string to be used in the test.
    /// Runs <paramref name="initializeDatabase"/> function for the first test in a sequence.
    /// </summary>
    /// <param name="databaseHash">
    /// Hash that uniquely identifies your database structure + seed data.
    /// Note that if <see cref="UseMd5Hash"/> is set to false, the length of the hash should be under 30 symbols.
    /// </param>
    /// <param name="initializeDatabase">
    /// Function that should create DB schema and seed the data.
    /// Receives PostgreSQL connection string.
    /// </param>
    /// <returns>Connection string to a copy of initialized database to be used in tests</returns>
    public override async Task<string> GetConnectionString(
        string databaseHash,
        Func<string, Task> initializeDatabase
    )
    {
        if (UseMd5Hash)
            databaseHash = Md5Hasher.CreateMD5(databaseHash);

        await InitializationTasks
            .GetOrAdd(
                databaseHash,
                async (key) =>
                {
                    CreateTemplateDto templateConfig = await _integreSqlClient
                        .InitializeTemplate(databaseHash)
                        .ConfigureAwait(false);

                    try
                    {
                        if (templateConfig == null)
                            return;

                        string templateConnectionString = GetConnectionString(
                            templateConfig.Database.Config,
                            "",
                            0
                        );
                        await initializeDatabase(templateConnectionString).ConfigureAwait(false);

                        await _integreSqlClient
                            .FinalizeTemplate(databaseHash)
                            .ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        await _integreSqlClient.DiscardTemplate(databaseHash).ConfigureAwait(false);
                        throw;
                    }
                }
            )
            .ConfigureAwait(false);

        GetDatabaseDto newDatabase = await _integreSqlClient
            .GetTestDatabase(databaseHash)
            .ConfigureAwait(false);
        return GetConnectionString(
            newDatabase.Database.Config,
            newDatabase.Database.TemplateHash,
            newDatabase.Id
        );
    }

    /// <summary>
    /// Returns test database to a pool (which allows consequent tests to reuse this database).
    /// Note that you need to clean up database by yourself before returning it to the pool!
    /// If you return a dirty database, consequent tests might fail!
    /// If you don't want to clean it up, just don't use this function.
    /// Dirty databases are automatically deleted by IntegreSQL once database number exceeds a certain limit (500 by default).
    /// </summary>
    public override async Task ReturnDatabase(string connectionString)
    {
        var connectionStringInfo = ConnectionStringInfos[connectionString];

        await _integreSqlClient.ReturnTestDatabase(
            connectionStringInfo.Hash,
            connectionStringInfo.Id
        );
    }

    public override void UseProvider(DbContextOptionsBuilder options, string connectionString)
    {
        options.UseNpgsql(connectionString);
    }

    protected override void PerformBasicSeedingOperations(DbContext dbContext)
    {
        ContextHelper.ReloadTypesForEnumSupport(dbContext);
    }

    private string GetConnectionString(Config databaseConfig, string hash, int id)
    {
        if (UseMd5Hash)
            hash = Md5Hasher.CreateMD5(hash);

        var builder = new NpgsqlConnectionStringBuilder()
        {
            Host = ConnectionStringOverride?.Host ?? databaseConfig.Host,
            Port = ConnectionStringOverride?.Port ?? databaseConfig.Port,
            Database = databaseConfig.Database,
            Username = ConnectionStringOverride?.Username ?? databaseConfig.Username,
            Password = ConnectionStringOverride?.Password ?? databaseConfig.Password,
            Pooling = false,
            //KeepAlive = 0,
        };
        var connectionString = builder.ToString();
        ConnectionStringInfos.TryAdd(connectionString, new ConnectionStringInfo(hash, id));

        return connectionString;
    }
}
