using System.Text.Json.Serialization;

namespace MccSoft.IntegreSqlClient.Dto;

public partial class Config
{
    [JsonPropertyName("host")]
    public string Host { get; set; }

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("database")]
    public string Database { get; set; }
}
