using System.Globalization;
using I18Next.Net.Backends;
using I18Next.Net.Extensions;
using I18Next.Net.Plugins;
using MccSoft.TemplateApp.App.Utils.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MccSoft.TemplateApp.App.Setup;

public static class SetupLocalization
{
    public static void AddLocalization(IServiceCollection services)
    {
        services.AddI18NextLocalization(
            i18N =>
                i18N.AddLanguageDetector<ThreadLanguageDetector>()
                    .Configure(o => o.DetectLanguageOnEachTranslation = true)
                    .AddBackend(new JsonFileBackend("Dictionaries"))
        );
        services.AddSingleton<IConfigureOptions<MvcOptions>, ConfigureModelBindingLocalization>();
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
    }
}
