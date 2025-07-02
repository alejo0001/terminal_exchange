using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class FlowDto
{
    public ProcessType ProcessType { get; set; }
    public int? TagId { get; set; }
    public int TotalSteps { get; set; }
}
