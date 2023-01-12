using System.Text.Json;
using Microsoft.EntityFrameworkCore;

[Owned]
[Index("Int")]
[Index("String")]
public class WebHookAdditionalData
{
    /// <summary>
    /// Required for EF
    /// </summary>
    protected WebHookAdditionalData() { }

    public WebHookAdditionalData(
        int? @int = null,
        string? @string = null,
        JsonDocument? json = null
    )
    {
        Int = @int;
        String = @string;
        Json = json;
    }

    public int? Int { get; private set; }
    public string? String { get; private set; }
    public JsonDocument? Json { get; private set; }
}
