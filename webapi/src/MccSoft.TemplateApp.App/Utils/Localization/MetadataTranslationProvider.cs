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

    public void CreateValidationMetadata(IList<object> attributes)
    {
        foreach (var attribute in attributes)
        {
            if (attribute is ValidationAttribute tAttr)
            {
                if (tAttr.ErrorMessageResourceName != null && tAttr.ErrorMessage == null)
                {
                    var functionField = typeof(ValidationAttribute).GetField(
                        "_errorMessageResourceAccessor",
                        BindingFlags.NonPublic | BindingFlags.Instance
                    );

                    Func<string> localizer = () =>
                        _localizer["Frontend:Server_Errors." + tAttr.ErrorMessageResourceName];
                    functionField.SetValue(attribute, localizer);
                }
            }
        }
    }
}
