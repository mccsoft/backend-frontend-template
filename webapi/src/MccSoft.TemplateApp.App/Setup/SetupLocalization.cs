using System.Globalization;
using I18Next.Net.Backends;
using I18Next.Net.Extensions;
using I18Next.Net.Plugins;
using MccSoft.TemplateApp.App.Utils.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MccSoft.TemplateApp.App.Setup;

public static partial class SetupLocalization
{
    public static void AddLocalization(WebApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddI18NextLocalization(
            i18N =>
                i18N.AddLanguageDetector<ThreadLanguageDetector>()
                    .Configure(o => o.DetectLanguageOnEachTranslation = true)
                    .AddBackend(new JsonFileBackend("Dictionaries"))
        );
        services.AddSingleton<IConfigureOptions<MvcOptions>, ConfigureModelBindingLocalization>();

        // If you'd like to modify this class, consider adding your custom code in the SetupLocalization.partial.cs
        // This will make it easier to pull changes from Template when Template is updated
        // (actually this file will be overwritten by a file from template, which will make your changes disappear)
        AddProjectSpecifics(builder);
    }

    public static void UseLocalization(IApplicationBuilder app)
    {
        app.UseRequestLocalization(options =>
        {
            options.SupportedCultures = new[] { "en", "fr", "de" }
                .Select(
                    lang =>
                        new CultureInfo(lang)
                        {
                            // we need to manually specify NumberFormat, because otherwise for DE browsers ASP.NET Core treats FormData 1.2 as 12.
                            NumberFormat = CultureInfo.InvariantCulture.NumberFormat,
                        }
                )
                .ToList();
            options.SetDefaultCulture("en");
        });

        // If you'd like to modify this class, consider adding your custom code in the SetupLocalization.partial.cs
        // This will make it easier to pull changes from Template when Template is updated
        // (actually this file will be overwritten by a file from template, which will make your changes disappear)
        UseProjectSpecifics(app);
    }

    static partial void AddProjectSpecifics(WebApplicationBuilder builder);

    static partial void UseProjectSpecifics(IApplicationBuilder app);
}
