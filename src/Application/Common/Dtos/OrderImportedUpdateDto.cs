using System;

namespace CrmAPI.Application.Common.Dtos;

public class OrderImportedUpdateDto
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