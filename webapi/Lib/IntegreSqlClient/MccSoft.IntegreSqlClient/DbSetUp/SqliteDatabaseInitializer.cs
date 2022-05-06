using System;
using System.IO;
using System.Threading.Tasks;

namespace MccSoft.IntegreSqlClient.DbSetUp;

/// <summary>
/// Class that initializes the template database and returns connection string to a fresh instance every time.
/// </summary>
public class SqliteDatabaseInitializer : IDatabaseInitializer
{
    public SqliteDatabaseInitializer(Uri integreSqlUri = null) { }

    private static readonly LazyConcurrentDictionary<string, Task> InitializationTasks = new();
    public static bool UseMd5Hash = true;

    public async Task<string> GetConnectionString(
        string databaseHash,
        Func<string, Task> initializeDatabase
    )
    {
        if (UseMd5Hash)
            databaseHash = Md5Hasher.CreateMD5(databaseHash);

        var templateDatabaseName = $"backup_{databaseHash}.sqlite";

        await InitializationTasks.GetOrAdd(
            databaseHash,
            async (key) =>
            {
                if (File.Exists(templateDatabaseName))
                {
                    return;
                }
                await initializeDatabase(templateDatabaseName);
            }
        );

        var databaseFileName = $"test-{Guid.NewGuid()}.sqlite";
        File.Copy(templateDatabaseName, databaseFileName);

        return databaseFileName;
    }

    /// <summary>
    /// Returns test database to a pool
    /// </summary>
    public async Task ReturnDatabase(string connectionString)
    {
        File.Delete(connectionString);
    }
}
