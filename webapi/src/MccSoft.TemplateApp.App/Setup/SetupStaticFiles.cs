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

    /// <summary>
    /// Responds with 404 if client request file from /static/ folder which is not found.
    /// This is needed to correctly handle missing chunks on frontend (when old version of SPA tries to download chunks which are not already present on a server)
    /// </summary>
    public static void Use404ForMissingStaticFiles(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
            return;

        app.Use(
            (context, next) =>
            {
                // If we have an Endpoint, then this is a deferred match - just noop.
                if (context.GetEndpoint() != null)
                {
                    return next();
                }

                // This means static files were not found.
                // If request is for /static/* file, then return 404.
                if (
                    context.Request.Path.StartsWithSegments("/static")
                    || context.Request.Path.StartsWithSegments("/assets")
                )
                {
                    context.Response.StatusCode = 404;
                    return Task.CompletedTask;
                }

                return next();
            }
        );
    }
}
