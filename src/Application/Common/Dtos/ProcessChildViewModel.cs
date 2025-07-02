using System;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class ProcessChildViewModel : IMapFrom<Process>
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ContactId { get; set; }
    public int OrdersImportedId { get; set; }
    public string Type { get; set; }
    public string Status { get; set; }
    public string Outcome { get; set; }
    public string Description { get; set; }
    public DateTime Created { get; set; }
    public string? Colour { get; set; }
    public OrdersImportedDto OrdersImported { get; set; }
    public DiscardReasonProcessDto DiscardReasonProcess { get; set; }
    public University University { get; set; }
}