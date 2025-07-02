namespace CrmAPI.Infrastructure.Settings;

public class CalendarApiSettings
{
    public const string SectionName = "Calendar";
    
    public string Endpoint { get; set; } = null!;
}