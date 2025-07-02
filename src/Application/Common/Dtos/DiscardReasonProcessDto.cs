using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class DiscardReasonProcessDto : IMapFrom<DiscardReasonProcess>
{
    public int DiscardReasonId { get; set; }

    public DiscardReasonDto DiscardReason { get; set; }

    public int ProcessId { get; set; }

    public string Observations { get; set; }
}