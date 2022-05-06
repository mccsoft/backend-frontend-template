using System.Text.Json.Serialization;

namespace MccSoft.IntegreSqlClient.Dto;

public partial class Database
{
    [JsonPropertyName("templateHash")]
    public string TemplateHash { get; set; }

    [JsonPropertyName("config")]
    public Config Config { get; set; }
}
