using System;
using System.IO;
using System.Threading.Tasks;
using MccSoft.WebApi.Patching;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

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

    public static async Task AddOpenIdDictApplicationsFromConfiguration(
        this IApplicationBuilder applicationBuilder,
        string configurationSection = "OpenId:Clients"
    )
    {
        using IServiceScope scope = applicationBuilder.ApplicationServices.CreateScope();
        IServiceProvider serviceProvider = scope.ServiceProvider;
        IOpenIddictApplicationManager manager =
            serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        var clients = configuration
            .GetSection(configurationSection)
            .Get<OpenIddictApplicationDescriptor[]>();

        foreach (var client in clients)
        {
            var clientObject = await manager.FindByClientIdAsync(client.ClientId!);
            // See OpenIddictConstants.Permissions for available permissions

            if (clientObject is null)
            {
                await manager.CreateAsync(client);
            }
            else
            {
                client.Type ??= "public";
                await manager.PopulateAsync(clientObject, client);
                await manager.UpdateAsync(clientObject);
            }
        }
    }
}
