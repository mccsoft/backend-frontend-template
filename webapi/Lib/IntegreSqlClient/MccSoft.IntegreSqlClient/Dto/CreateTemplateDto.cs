using System.Text.Json.Serialization;

namespace MccSoft.IntegreSqlClient.Dto;

public partial class CreateTemplateDto
{
    [JsonPropertyName("database")]
    public Database Database { get; set; }
}
