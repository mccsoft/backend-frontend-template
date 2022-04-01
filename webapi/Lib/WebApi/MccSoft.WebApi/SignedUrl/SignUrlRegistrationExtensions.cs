using System;
using Microsoft.Extensions.DependencyInjection;

namespace MccSoft.WebApi.SignedUrl
{
    public static class SignUrlRegistrationExtensions
    {
        public const string SignUrlPolicyName = "SignUrl";

        /// <summary>
        /// Adds an infrastructure to handle SignedUrls.
        /// </summary>
        public static IServiceCollection AddSignUrl(
            this IServiceCollection services,
            string secret,
            Action<SignUrlOptions> configureAction = null
        )
        {
            if (string.IsNullOrEmpty(secret))
            {
                throw new ArgumentNullException("Secret should be a non-empty string");
            }

            if (secret.Length < 16)
            {
                throw new ArgumentException("Secret length should be at least 128bit");
            }

            services.AddSingleton<SignUrlHelper>();
            services.Configure<SignUrlOptions>(
                x =>
                {
                    x.Secret = secret;
                    configureAction?.Invoke(x);
                }
            );
            return services;
        }
    }
}
