using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using MccSoft.IntegreSqlClient.Dto;
using Npgsql;

namespace MccSoft.IntegreSqlClient.DbSetUp;

/// <summary>
/// Class that initializes the template database and returns connection string to a fresh instance every time.
/// </summary>
public class NpgsqlDatabaseInitializer : IDatabaseInitializer
{
    private readonly IntegreSqlClient _integreSqlClient;

    /// <summary>
    /// Sets postgresql host to be used in returned connection strings.
    /// If not set the host returned from IntegreSQL will be used.
    /// </summary>
    public static string PostgreSqlHost = null;

    public static int? PostgreSqlPort = null;
    public static bool UseMd5Hash = true;

    private record ConnectionStringInfo(string hash, int id);

    private static ConcurrentDictionary<string, ConnectionStringInfo> ConnectionStringInfos = new();

    public NpgsqlDatabaseInitializer(Uri integreSqlUri = null)
    {
        integreSqlUri ??= new Uri("http://localhost:5000/api/v1/");
        _integreSqlClient = new IntegreSqlClient(integreSqlUri);
    }

    private static readonly LazyConcurrentDictionary<string, Task> InitializationTasks = new();

    /// <summary>
    /// Returns a PostgreSQL connection string to be used in the test.
    /// Runs <see cref="initializeDatabase"/> function for the first test in a sequence. 
    /// </summary>
    /// <param name="databaseHash">Hash that uniquely identifies your database structure + seed data</param>
    /// <param name="initializeDatabase">
    /// Function that should create DB schema and seed the data.
    /// Receives PostgreSQL connection string.
    /// </param>
    /// <returns></returns>
    public async Task<string> GetConnectionString(
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
    /// Returns test database to a pool
    /// </summary>
    public async Task ReturnDatabase(string connectionString)
    {
        var connectionStringInfo = ConnectionStringInfos[connectionString];

        await _integreSqlClient.ReturnTestDatabase(
            connectionStringInfo.hash,
            connectionStringInfo.id
        );
    }

    private string GetConnectionString(Config databaseConfig, string hash, int id)
    {
        if (UseMd5Hash)
            hash = Md5Hasher.CreateMD5(hash);

        var builder = new NpgsqlConnectionStringBuilder()
        {
            Host = PostgreSqlHost ?? databaseConfig.Host,
            Port = PostgreSqlPort ?? databaseConfig.Port,
            Database = databaseConfig.Database,
            Username = databaseConfig.Username,
            Password = databaseConfig.Password,
            Pooling = false,
            //KeepAlive = 0,
        };
        var connectionString = builder.ToString();
        ConnectionStringInfos.TryAdd(connectionString, new ConnectionStringInfo(hash, id));

        return connectionString;
    }
}
