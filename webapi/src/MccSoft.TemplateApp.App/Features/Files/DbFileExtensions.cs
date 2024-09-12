using System.Linq.Expressions;
using MccSoft.TemplateApp.App.Features.Files.Dto;
using MccSoft.TemplateApp.Domain;
using NeinLinq;

namespace MccSoft.TemplateApp.App.Features.Files;

public static class DbFileExtensions
{
    [InjectLambda]
    public static FileInfoDto ToFileInfoDto(this DbFile dbFile)
    {
        return ToFileInfoDtoExpressionCompiled.Value(dbFile);
    }

    private static readonly Lazy<Func<DbFile, FileInfoDto>> ToFileInfoDtoExpressionCompiled =
        new(() => ToFileInfoDto().Compile());

    public static Expression<Func<DbFile, FileInfoDto>> ToFileInfoDto() =>
        dbFile =>
            new FileInfoDto
            {
                Id = dbFile.Id.ToString(),
                FileName = dbFile.FileName,
                Size = dbFile.Size,
            };
}
