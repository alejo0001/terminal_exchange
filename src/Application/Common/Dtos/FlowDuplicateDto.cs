using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class FlowDuplicateDto
{
    public ProcessType OriginProcessType { get; set; }
    public int OriginTagId { get; set; }
    public ProcessType ProcessType { get; set; }
    public int TagId { get; set; }
    public string TagName { get; set; }
}