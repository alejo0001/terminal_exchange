namespace CrmAPI.Infrastructure.Settings;

/// <summary>
///     Type to carry only database Connection Strings. Do not mingle other config into it, because its data is considered
///     sensitive and therefore provided by AZ Key Vault service. <br />
///     In Local environment User Secret logic can be plugged-in so that consuming code would not need to be changed.
/// </summary>
/// <remarks>
///     It is .NET application well-known settings section. Please change it only according to those conventions.
/// </remarks>
public class ConnectionStringsSettings
{
    public const string SectionName = "ConnectionStrings";

    public string Intranet { get; set; } = string.Empty;

    /// <summary>
    ///     Synonym: Potenciales.
    /// </summary>
    public string Leads { get; set; } = string.Empty;

    /// <summary>
    ///     Synonym: Telemarketing.
    /// </summary>
    public string TLMK { get; set; } = string.Empty;

    /// <summary>
    ///     Synonym: Course Database. This is original DB, e.g. for <em>University</em>.
    /// </summary>
    public string WebDatabase { get; set; } = string.Empty;

    /// <summary>
    ///     Synonym: Course Database. This is for the <em>FP (Formación Profesional)</em>.
    /// </summary>
    public string WebFpDatabase { get; set; } = string.Empty;

    /// <summary>
    ///     TEMPORAL! HACK, DO NOT USE, ASK TO DELETE FROM KVS ASAP!
    /// </summary>
    public bool SendEmail { get; set; }
}
