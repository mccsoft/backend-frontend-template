using System;
using Microsoft.Data.Sqlite;

namespace MccSoft.Testing.SqliteUtils
{
    /// <summary>
    /// Keeps one connection to the shared Sqlite in-memory DB open for
    /// the entire application lifetime to prevent the DB from being destroyed.
    /// See https://www.sqlite.org/inmemorydb.html
    /// </summary>
    public class SqliteConnectionHolder : IDisposable
    {
        public class ConnectionString
        {
            public string Str;
        }

        private readonly SqliteConnection _connection;
        public SqliteConnectionHolder(ConnectionString connectionString)
        {
            _connection = new SqliteConnection(connectionString.Str);
            _connection.Open();
        }

        public void BackupDatabase(string backupFileName)
        {
            var newConnection = new SqliteConnection($"DataSource={backupFileName};Pooling=false");
            _connection.BackupDatabase(newConnection);
        }

        public void Dispose()
        {
            _connection.Close();
        }
    }
}
