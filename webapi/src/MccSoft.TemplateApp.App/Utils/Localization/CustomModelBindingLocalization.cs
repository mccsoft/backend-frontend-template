using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace MccSoft.TemplateApp.App.Utils.Localization;

public class ConfigureModelBindingLocalization : IConfigureOptions<MvcOptions>
{
    private readonly IServiceScopeFactory _serviceFactory;

    public ConfigureModelBindingLocalization(IServiceScopeFactory serviceFactory)
    {
        _serviceFactory = serviceFactory;
    }

    public void Configure(MvcOptions options)
    {
        using var scope = _serviceFactory.CreateScope();
        var provider = scope.ServiceProvider;

        // localize Validation attributes
        var validationLocalizer = provider.GetRequiredService<
            IStringLocalizer<MetadataTranslationProvider.DataAnnotationLocalizer>
        >();
        options.ModelMetadataDetailsProviders.Add(
            new MetadataTranslationProvider(validationLocalizer)
        );

        var localizer = provider.GetRequiredService<
            IStringLocalizer<ConfigureModelBindingLocalization>
        >();
        options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
            (value, property) =>
                localizer["ModelBindingErrors:AttemptedValueIsInvalid", new { value, property }]
        );

        options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(
            (parameterName) =>
                localizer["ModelBindingErrors:MissingBindRequiredValue", new { parameterName }]
        );

        options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(
            () => localizer["ModelBindingErrors:MissingKeyOrValue"]
        );

        options.ModelBindingMessageProvider.SetMissingRequestBodyRequiredValueAccessor(
            () => localizer["ModelBindingErrors:MissingRequestBodyRequired"]
        );

        options.ModelBindingMessageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor(
            (value) =>
                localizer["ModelBindingErrors:NonPropertyAttemptedValueIsInvalid", new { value }]
        );

        options.ModelBindingMessageProvider.SetNonPropertyUnknownValueIsInvalidAccessor(
            () => localizer["ModelBindingErrors:NonPropertyUnknownValueIsInvalid"]
        );

        options.ModelBindingMessageProvider.SetNonPropertyValueMustBeANumberAccessor(
            () => localizer["ModelBindingErrors:NonPropertyValueMustBeANumber"]
        );

        options.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor(
            (propertyName) =>
                localizer["ModelBindingErrors:UnknownValueIsInvalid", new { propertyName }]
        );

        options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
            (value) => localizer["ModelBindingErrors:ValueIsInvalid", new { value }]
        );

        options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
            (propertyName) =>
                localizer["ModelBindingErrors:ValueMustBeANumber", new { propertyName }]
        );

        options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
            (value) => localizer["ModelBindingErrors:ValueMustNotBeNull", new { value }]
        );
    }
}
