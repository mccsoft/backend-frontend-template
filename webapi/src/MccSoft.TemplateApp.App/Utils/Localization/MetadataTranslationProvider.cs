using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Localization;

namespace MccSoft.TemplateApp.App.Utils.Localization;

public class MetadataTranslationProvider : IValidationMetadataProvider
{
    private readonly IStringLocalizer<DataAnnotationLocalizer> _localizer;

    /// <summary>
    /// just a marker class to use in StringLocalizer
    /// </summary>
    public class DataAnnotationLocalizer { }

    public MetadataTranslationProvider(IStringLocalizer<DataAnnotationLocalizer> localizer)
    {
        _localizer = localizer;
    }

    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        CreateValidationMetadata(context.ValidationMetadata.ValidatorMetadata);
    }

    private FieldInfo _functionField = typeof(ValidationAttribute).GetField(
        "_errorMessageResourceAccessor",
        BindingFlags.NonPublic | BindingFlags.Instance
    );

    private PropertyInfo _customErrorMessageSetProperty = typeof(ValidationAttribute).GetProperty(
        "CustomErrorMessageSet",
        BindingFlags.NonPublic | BindingFlags.Instance
    );

    public void CreateValidationMetadata(IList<object> attributes)
    {
        foreach (var attribute in attributes)
        {
            if (attribute is ValidationAttribute tAttr)
            {
                if (tAttr.ErrorMessage == null)
                {
                    var resourceName =
                        tAttr.ErrorMessageResourceName
                        ?? tAttr.GetType().Name.Replace("Attribute", "");

                    Func<string> localizer = () => _localizer["ValidationErrors:" + resourceName];
                    _functionField.SetValue(attribute, localizer);
                    _customErrorMessageSetProperty.SetValue(attribute, true);
                }
            }
        }
    }
}
