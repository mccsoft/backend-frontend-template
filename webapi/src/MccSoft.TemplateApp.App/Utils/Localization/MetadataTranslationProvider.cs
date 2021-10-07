using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using MccSoft.WebApi.Domain.Helpers;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Localization;

namespace MccSoft.TemplateApp.App.Utils.Localization
{
    public class MetadataTranslationProvider : IValidationMetadataProvider
    {
        private readonly IStringLocalizer<DataAnnotationLocalizer> _localizer;

        /// <summary>
        /// just a marker class to use in StringLocalizer
        /// </summary>
        public class DataAnnotationLocalizer
        {
        }

        public MetadataTranslationProvider(IStringLocalizer<DataAnnotationLocalizer> localizer)
        {
            _localizer = localizer;
        }

        public void CreateValidationMetadata(ValidationMetadataProviderContext context)
        {
            foreach (var attribute in context.ValidationMetadata.ValidatorMetadata)
            {
                if (attribute is ValidationAttribute tAttr)
                {
                    if (tAttr.ErrorMessage == null && tAttr.ErrorMessageResourceName == null)
                    {
                        var attributeType = tAttr.GetType();
                        var name = attributeType.Name;
                        if (attributeType.Name == nameof(RequiredOrMissingAttribute))
                        {
                            name = nameof(RequiredAttribute);
                        }

                        name = name.Replace("Attribute", "");

                        var functionField = typeof(ValidationAttribute).GetField(
                            "_errorMessageResourceAccessor",
                            BindingFlags.NonPublic | BindingFlags.Instance
                        );
                        Func<string> localizer = () => _localizer["ValidationErrors:" + name];
                        functionField.SetValue(tAttr, localizer);
                        // tAttr.ErrorMessage = "ValidationErrors:" + name;
                    }
                }
            }
        }
    }
}
