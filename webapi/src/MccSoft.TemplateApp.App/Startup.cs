// using System.Runtime.CompilerServices;
// using Microsoft.AspNetCore.Mvc;
// using ILogger = Microsoft.Extensions.Logging.ILogger;
//
// namespace MccSoft.TemplateApp.App
// {
//     public class Startup
//     {
//         private const string _healthCheckUrl = "/health";
//
//         public static async Task Configure(
//             IApplicationBuilder app,
//             IHostApplicationLifetime appLifetime,
//             IHostEnvironment hostEnvironment,
//             IConfiguration configuration,
//             ILogger logger
//         )
//         {
//             // app.UseDefaultFiles();
//             // app.UseStaticFiles();
//             // app.UseSpaStaticFiles();
//
//
//             // app.UseSpa(spa =>
//             // {
//             //     // https://github.com/dotnet/aspnetcore/issues/3147#issuecomment-435617378
//             //     spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions()
//             //     {
//             //         OnPrepareResponse = ctx =>
//             //         {
//             //             // Do not cache implicit `/index.html`
//             //             var headers = ctx.Context.Response.GetTypedHeaders();
//             //             headers.CacheControl = new CacheControlHeaderValue
//             //             {
//             //                 Public = true,
//             //                 MaxAge = TimeSpan.FromDays(0)
//             //             };
//             //         }
//             //     };
//             //
//             //     if (hostEnvironment.IsDevelopment())
//             //     {
//             //         spa.Options.SourcePath = Path.GetFullPath("../../../frontend");
//             //         // spa.UseReactDevelopmentServer("npm-start");
//             //         spa.UseProxyToSpaDevelopmentServer("http://localhost:3149/");
//             //     }
//             //     else
//             //     {
//             //         spa.Options.SourcePath = "wwwroot";
//             //     }
//             // });
//             // _httpContextAccessor =
//             //     app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
//         }
//     }
// }
