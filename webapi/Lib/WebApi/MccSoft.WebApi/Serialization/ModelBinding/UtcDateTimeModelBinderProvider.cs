using System;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MccSoft.WebApi.Serialization.ModelBinding;

/// <summary>
/// Model Binder to make sure all incoming DateTime GET parameters are either
/// - converted to UTC (if passed parameter contains a timezone)
/// - assumed to be in UTC (if passed parameter doesn't contain a timezone information)
/// This is to comply with UTC everywhere model, to have all DateTime on backend with DateTimeKind == Utc
/// </summary>
public class UtcDateTimeModelBinderProvider : IModelBinderProvider
{
    private const DateTimeStyles _supportedStyles =
        DateTimeStyles.AssumeUniversal
        | DateTimeStyles.AdjustToUniversal
        | DateTimeStyles.AllowWhiteSpaces;

    /// <inheritdoc />
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var modelType = context.Metadata.UnderlyingOrModelType;
        var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
        if (modelType == typeof(DateTime) || modelType == typeof(DateTime?))
        {
            return new DateTimeModelBinder(_supportedStyles, loggerFactory);
        }

        return null;
    }
}
