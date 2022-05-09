#nullable enable
namespace MccSoft.IntegreSqlClient.DbSetUp;

/// <summary>
/// Overrides certain connection string settings returned from IntegreSQL.
/// All non-null values will be used to override IntegreSQL settings.
/// Null values will be omitted (values from IntegreSQL will be used).
/// </summary>
public class ConnectionStringOverride
{
    public string? Host { get; set; }
    public int? Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}
