namespace MccSoft.TemplateApp.Domain;

/// <summary>
/// Used to store some arbitrary metadata regarding the file
/// All fields here should be nullable, since it's possible that files do not have metadata at all
/// </summary>
public class DbFileMetadata
{
    public string? ExternalId { get; set; }
}
