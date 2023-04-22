using Microsoft.AspNetCore.Mvc;

namespace MccSoft.WebApi.Serialization.FromQueryJson;

/// <summary>
/// Allows to use JSON-serialized DTO in Query parameters of HTTP requests.
/// </summary>
public class FromJsonQueryAttribute : ModelBinderAttribute
{
    public FromJsonQueryAttribute()
    {
        BinderType = typeof(JsonQueryBinder);
    }
}
