using System.Text.Json;

namespace MccSoft.TemplateApp.Http;

public interface IBaseClient
{
    public JsonSerializerOptions JsonSerializerSettings { get; }
}
