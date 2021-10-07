using System;
using System.Diagnostics.CodeAnalysis;
using MccSoft.Mailing.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MccSoft.Mailing
{
    public static class MailingExtensions
    {
        private const string BasePath = "Emails";

        public static readonly Func<
            EmailModelBase,
            EmailTemplateType,
            string
        > DefaultEmailModelPathConverter = (model, templateType) =>
        {
            string typeName = model.GetType().Name;
            string folderName = typeName.Replace("EmailModel", "")
                .Replace("MailModel", "")
                .Replace("Model", "");
            return BasePath
                + "/"
                + folderName
                + "/"
                + typeName
                + "_"
                + templateType.ToString().ToLower();
        };

        /// <summary>
        /// Registers IMailSender and IRazorRenderService interfaces
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="configuration">The configuration being bound. Should contain <see cref="MailSenderOptions"/>!</param>
        /// <param name="getEmailViewPath">Function to get path to razor template for passed model</param>
        /// <returns></returns>
        public static IServiceCollection AddMailing(
            [NotNull] this IServiceCollection services,
            IConfiguration configuration,
            Func<EmailModelBase, EmailTemplateType, string> getEmailViewPath = null
        ) {
            services.Configure<MailSenderOptions>(configuration);
            return services.AddSingleton(
                    new MailSettings()
                    {
                        EmailViewPathProvider = getEmailViewPath ?? DefaultEmailModelPathConverter
                    }
                )
                .AddScoped<IMailSender, MailSender>()
                .AddScoped<IRazorRenderService, RazorRenderService>();
        }
    }
}
