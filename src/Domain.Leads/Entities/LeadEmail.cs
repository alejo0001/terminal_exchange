namespace CrmAPI.Domain.Leads.Entities;

public class LeadEmail
{
    public int id { get; set; }
    public int contactid { get; set; }
    public string email { get; set; }
}