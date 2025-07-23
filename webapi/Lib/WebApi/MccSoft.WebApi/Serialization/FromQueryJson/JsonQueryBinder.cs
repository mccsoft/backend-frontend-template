using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Logging;

namespace MccSoft.WebApi.Serialization.FromQueryJson;

/// <summary>
/// Model binder for <see cref="FromJsonQueryAttribute"/>.
/// Allows to use JSON-serialized DTO in Query parameters of HTTP requests.
/// </summary>
public class JsonQueryBinder : IModelBinder
{
    private readonly ILogger<JsonQueryBinder> _logger;
    private readonly IObjectModelValidator _validator;

    public JsonQueryBinder(IObjectModelValidator validator, ILogger<JsonQueryBinder> logger)
    {
        _validator = validator;
        _logger = logger;
    }

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider.GetValue(bindingContext.FieldName).FirstValue;
        if (value == null)
        {
            return Task.CompletedTask;
        }

        try
        {
            var parsed = JsonSerializer.Deserialize(
                value,
                bindingContext.ModelType,
                DefaultJsonSerializer.DeserializationOptions
            );
            bindingContext.Result = ModelBindingResult.Success(parsed);

            if (parsed != null)
            {
                _validator.Validate(
                    bindingContext.ActionContext,
                    validationState: bindingContext.ValidationState,
                    prefix: string.Empty,
                    model: parsed
                );
            }
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Failed to bind parameter '{FieldName}'", bindingContext.FieldName);
            bindingContext.ActionContext.ModelState.TryAddModelError(
                key: e.Path,
                exception: e,
                bindingContext.ModelMetadata
            );
        }
        catch (Exception e) when (e is FormatException || e is OverflowException)
        {
            _logger.LogError(e, "Failed to bind parameter '{FieldName}'", bindingContext.FieldName);
            bindingContext.ActionContext.ModelState.TryAddModelError(
                string.Empty,
                e,
                bindingContext.ModelMetadata
            );
        }

        return Task.CompletedTask;
    }
}
