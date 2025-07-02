using System;
using System.Reflection;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using CrmAPI.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CrmAPI.Infrastructure.Configuration;

/// <summary>
///     Application Configuration bootstrapping.
/// </summary>
/// <remarks>
///     Idea is to follow Microsoft recommended patterns and also abstract away configuration origin details, application
///     should only interact with configuration:
///     <c>IConfiguration, IOptions&lt;T&gt;, IOptionsSnapshot&lt;T&gt;, IOptionsMonitor&lt;T&gt;</c>.<br />
///     TODO: Migrate to IntranetExtensions package. <br />
///     TODO: Azure App Configuration Service. <br />
///     TODO: implement interactive auth for developers and for application.
/// </remarks>
public static class ConfigurationExtensions
{
    public const double KeyVaultDefaultReloadMinutes = 1;

    /// <summary>
    ///     It must be among the first bootstrapping steps of the whole app, because it impacts configuration composition.
    /// </summary>
    /// <remarks>
    ///     Current implementation includes setting up Azure Key Vault service.<br />
    /// </remarks>
    /// <param name="configManager">
    ///     This is more suitable type instead of interfaces, because it allows to access config and
    ///     also perform setup actions on it.
    /// </param>
    /// <param name="hostEnvironment"></param>
    /// <returns></returns>
    [UsedImplicitly]
    public static ConfigurationManager SetupAppConfiguration(
        this ConfigurationManager configManager,
        IHostEnvironment hostEnvironment)
    {
        configManager.AddAzureKeyVault();

        if (IsDevelopmentOverride(hostEnvironment))
        {
            // This is called "last" with purpose to override credentials from User Secrets Manager.
            configManager.AddUserSecrets(Assembly.GetEntryAssembly()!, true, true);
        }

        return configManager;
    }

    /// <summary>
    ///     Only <see cref="Environments.Production" /> and <see cref="Environments.Staging" /> will not be overriden.
    /// </summary>
    /// <param name="hostEnvironment"></param>
    /// <returns></returns>
    private static bool IsDevelopmentOverride(IHostEnvironment hostEnvironment) =>
        !(hostEnvironment.IsProduction() || hostEnvironment.IsStaging());

    /// <summary>
    ///     Encapsulates all Azure Key Vault setup peculiarities.
    /// </summary>
    /// <param name="configManager"></param>
    public static void AddAzureKeyVault(this ConfigurationManager configManager)
    {
        var settings = new KeyVaultSettings();
        configManager.Bind(KeyVaultSettings.SectionName, settings);

        var keyVaultUrl = new Uri($"https://{settings.Name}.vault.azure.net/");

        var credential = new ClientSecretCredential(
            settings.TenantId,
            settings.ClientId,
            settings.ClientSecret);

        var azureKeyVaultConfigurationOptions = new AzureKeyVaultConfigurationOptions
        {
            ReloadInterval = TimeSpan.FromMinutes(settings.ReloadIntervalMinutes ?? KeyVaultDefaultReloadMinutes),
        };

        configManager.AddAzureKeyVault(keyVaultUrl, credential, azureKeyVaultConfigurationOptions);
    }
}
