using MccSoft.TemplateApp.App.Features.Files.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MccSoft.TemplateApp.App.Features.Files;

[Authorize]
[Route("api/files")]
public class FilesController
{
    private readonly FileService _fileService;

    public FilesController(FileService fileService)
    {
        _fileService = fileService;
    }

    /// <summary>
    /// returns a file id
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<FileInfoDto> UploadFile(IFormFile file)
    {
        return await _fileService.Upload(file);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    // [ValidateSignedUrl]
    public async Task<FileStreamResult> DownloadFile(Guid id)
    {
        return await _fileService.Download(id);
    }

    [HttpDelete("{id}")]
    public async Task Delete(Guid id)
    {
        await _fileService.Delete(id);
    }
}
