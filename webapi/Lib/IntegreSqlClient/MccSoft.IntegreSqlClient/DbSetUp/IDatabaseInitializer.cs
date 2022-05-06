using System;
using System.Threading.Tasks;

namespace MccSoft.IntegreSqlClient.DbSetUp;

public interface IDatabaseInitializer
{
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
    Task<string> GetConnectionString(string databaseHash, Func<string, Task> initializeDatabase);

    /// <summary>
    /// Returns test database to a pool
    /// </summary>
    Task ReturnDatabase(string connectionString);
}
