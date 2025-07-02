using System;

namespace CrmAPI.Domain.Leads.Entities;

public class TMPRemesa
{
    public int OrderNumber { get; set; }
    public int? ProcessId { get; set; }
    public string? Email { get; set; }
    public string? Nif { get; set; }
    public string? Status { get; set; }
    public string? PaymentType { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime? TerminationDate { get; set; }
}