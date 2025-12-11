namespace MccSoft.TemplateApp.App.Features.Files.Dto;

public class FileInfoDto
{
    public string Id { get; set; }
    public string FileName { get; set; }
    public long Size { get; set; }

    public FileMetadataDto Metadata { get; set; }
}
