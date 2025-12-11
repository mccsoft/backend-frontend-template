using System;
using System.ComponentModel.DataAnnotations;
using MccSoft.DomainHelpers;

namespace MccSoft.TemplateApp.Domain;

public class DbFile
{
    protected DbFile() { }

    public DbFile(
        Guid id,
        string fileName,
        string pathOnDisk,
        byte[] hash,
        long size,
        DateTime createdAt
    )
    {
        Id = id;
        FileName = fileName;
        PathOnDisk = pathOnDisk;
        Hash = hash;
        Size = size;
        CreatedAt = createdAt;
    }

    public Guid Id { get; protected set; }

    [StringLength(256)]
    public string FileName { get; init; }
    public string PathOnDisk { get; init; }

    public byte[] Hash { get; set; }

    public long Size { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Could be used to store some arbitrary metadata regarding the file
    /// </summary>
    public DbFileMetadata? Metadata { get; set; }

    public static Specification<DbFile> HasId(Guid id) => new(nameof(HasId), p => p.Id == id, id);
}
