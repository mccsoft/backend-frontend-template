using System.Text.Json.Serialization;

namespace MccSoft.IntegreSqlClient.Dto;

public partial class GetDatabaseDto
{
    [JsonPropertyName("database")]
    public Database Database { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }
}
