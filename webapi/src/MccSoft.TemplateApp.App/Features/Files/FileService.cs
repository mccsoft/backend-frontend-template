using System.Security.Cryptography;
using MccSoft.LowLevelPrimitives;
using MccSoft.PersistenceHelpers;
using MccSoft.TemplateApp.App.Features.Files.Dto;
using MccSoft.TemplateApp.Domain;
using MccSoft.TemplateApp.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace MccSoft.TemplateApp.App.Features.Files;

public class FileService
{
    private readonly TemplateAppDbContext _dbContext;
    private readonly string _filesDirectory;

    public FileService(TemplateAppDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _filesDirectory = configuration.GetValue<string>("DefaultFileStorage");
    }

    public async Task<FileInfoDto> Upload(IFormFile file)
    {
        var id = Guid.NewGuid();
        var filePath = await UploadFileToDisk(id, file);

        var dbFile = new DbFile(
            id,
            file.FileName,
            filePath,
            ComputeSha2(filePath),
            new FileInfo(filePath).Length,
            DateTimeHelper.UtcNow
        );
        _dbContext.Files.Add(dbFile);
        await _dbContext.SaveChangesAsync();

        return await Get(dbFile.Id);
    }

    public async Task<FileInfoDto> Get(Guid id)
    {
        return await _dbContext.Files.GetOne(DbFile.HasId(id), x => x.ToFileInfoDto());
    }

    private string GetFilePath(Guid id)
    {
        var filePath = GetUploadedFilesFolder();
        filePath = Path.Combine(
            filePath,
            Path.Combine(id.ToString("D").Split('-')[0]),
            id.ToString("D")
        );
        return filePath;
    }

    public string GetUploadedFilesFolder()
    {
        var directory = Path.Combine(_filesDirectory, "Files");
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static byte[] ComputeSha2(string filePath)
    {
        byte[] sha2;
        using SHA256 mySha256 = SHA256.Create();
        FileInfo fInfo = new FileInfo(filePath);
        using FileStream fileStream = fInfo.Open(FileMode.Open);
        fileStream.Position = 0;
        sha2 = mySha256.ComputeHash(fileStream);
        fileStream.Close();

        return sha2;
    }

    private async Task<string> UploadFileToDisk(Guid fileId, IFormFile file)
    {
        // Upload file
        var filePath = GetFilePath(fileId);
        CreateFileDirectoryIfNotExists(filePath);

        await using var stream = File.Create(filePath);
        await file.CopyToAsync(stream);
        stream.Close();

        return filePath;
    }

    private static void CreateFileDirectoryIfNotExists(string filePath)
    {
        var dir = Path.GetDirectoryName(filePath);
        Directory.CreateDirectory(dir);
    }

    public async Task<FileStreamResult> Download(Guid id)
    {
        var file = await _dbContext.Files.GetOne(DbFile.HasId(id));
        var filePath = GetFilePath(id);
        return new FileStreamResult(File.OpenRead(filePath), "application/octet-stream")
        {
            FileDownloadName = file.FileName,
            LastModified = file.CreatedAt,
        };
    }

    public async Task Delete(Guid id)
    {
        var file = await _dbContext.Files.GetOne(DbFile.HasId(id));
        File.Delete(GetFilePath(id));
        _dbContext.Files.Remove(file);
        await _dbContext.SaveChangesAsync();
    }
}
