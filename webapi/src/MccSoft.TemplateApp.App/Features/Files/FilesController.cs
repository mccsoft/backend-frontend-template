using MccSoft.TemplateApp.App.Features.Files.Dto;
using MccSoft.WebApi.Pagination;
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
    [ProducesResponseType(typeof(FileStreamResult), 200)]
    // [ValidateSignedUrl]
    public async Task<FileStreamResult> DownloadFile(Guid id)
    {
        return await _fileService.Download(id);
    }

    [HttpGet("{id}")]
    public async Task<FileInfoDto> Get(Guid id)
    {
        return await _fileService.Get(id);
    }

    [HttpGet]
    public async Task<PagedResult<FileInfoDto>> Get([FromQuery] SearchFileDto searchDto)
    {
        return await _fileService.Get(searchDto);
    }

    [HttpDelete("{id}")]
    public async Task Delete(Guid id)
    {
        await _fileService.Delete(id);
    }
}
