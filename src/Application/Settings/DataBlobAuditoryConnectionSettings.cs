namespace CrmAPI.Application.Settings;

public class DataBlobAuditoryConnectionSettings
{
    public const string SectionName = "DataBlobAuditoryConnection";
    
    public string BaseUriAuditory { get; set; } = null!;

    public string ContainerNameAuditory { get; set; } = null!;

    public string AccessTokenAuditory { get; set; } = null!;

    public string FilenameAuditory { get; set; } = null!;

}