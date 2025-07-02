namespace CrmAPI.Infrastructure.Settings;

public class KeyVaultSettings
{
    public const string SectionName = "KeyVaultSettings";

    /// <summary>
    ///     Key Vault service instance name in Azure.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     The Microsoft Entra tenant (directory) ID of the service principal.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    ///     The client (application) ID of the service principal.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    ///     A client secret that was generated for the App Registration used to authenticate the client.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    ///     <inheritdoc
    ///         cref="Azure.Extensions.AspNetCore.Configuration.Secrets.AzureKeyVaultConfigurationOptions.ReloadInterval" />
    /// </summary>
    public double? ReloadIntervalMinutes { get; set; }
}
