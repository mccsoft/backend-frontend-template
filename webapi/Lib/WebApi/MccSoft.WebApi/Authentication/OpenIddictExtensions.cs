using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MccSoft.WebApi.Authentication;

public static class OpenIddictExtensions
{
    public static void AddSigningCertificateFromConfiguration(
        this OpenIddictServerBuilder options,
        IConfiguration configuration,
        string configurationPath = "OpenId:SigningCertificate"
    )
    {
        var signingCertificate = configuration.GetValue<OpenIdCertificateInfo>(configurationPath);
        if (
            !string.IsNullOrEmpty(signingCertificate?.Password)
            && !string.IsNullOrEmpty(signingCertificate?.Base64Certificate)
        )
        {
            options.AddSigningCertificate(
                new MemoryStream(Convert.FromBase64String(signingCertificate.Base64Certificate)),
                signingCertificate.Password
            );
        }
        else
        {
            options.AddDevelopmentSigningCertificate();
        }
    }

    public static void AddEncryptionCertificateFromConfiguration(
        this OpenIddictServerBuilder options,
        IConfiguration configuration,
        string configurationPath = "OpenId:EncryptionCertificate"
    )
    {
        var encryptionCertificate = configuration.GetValue<OpenIdCertificateInfo>(
            configurationPath
        );
        if (
            !string.IsNullOrEmpty(encryptionCertificate?.Password)
            && !string.IsNullOrEmpty(encryptionCertificate?.Base64Certificate)
        )
        {
            options.AddEncryptionCertificate(
                new MemoryStream(Convert.FromBase64String(encryptionCertificate.Base64Certificate)),
                encryptionCertificate.Password
            );
        }
        else
        {
            options.AddDevelopmentEncryptionCertificate();
        }
    }
}
