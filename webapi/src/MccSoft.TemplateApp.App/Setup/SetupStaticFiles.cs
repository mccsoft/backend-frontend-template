using Microsoft.Net.Http.Headers;

namespace MccSoft.TemplateApp.App.Setup;

public static partial class SetupStaticFiles
{
    public static StaticFileOptions DoNotCache =>
        new StaticFileOptions()
        {
            OnPrepareResponse = ctx =>
            {
                var headers = ctx.Context.Response.GetTypedHeaders();
                headers.CacheControl = new CacheControlHeaderValue
                {
                    Public = true,
                    MaxAge = TimeSpan.FromTicks(0),
                };
            }
        };

    public static StaticFileOptions CacheAll =>
        new StaticFileOptions()
        {
            OnPrepareResponse = ctx =>
            {
                var headers = ctx.Context.Response.GetTypedHeaders();
                headers.CacheControl = new CacheControlHeaderValue
                {
                    Public = true,
                    MaxAge = TimeSpan.FromDays(10000),
                };
            }
        };
}
