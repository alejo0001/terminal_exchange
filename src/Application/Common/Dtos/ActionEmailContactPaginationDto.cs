using System;
using CrmAPI.Application.Common.Mappings;
using Action = IntranetMigrator.Domain.Entities.Action;

namespace CrmAPI.Application.Common.Dtos;

public class ActionEmailContactPaginationDto : IMapFrom<Action>
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int? UserId { get; set; }
    public int ContactId { get; set; }
    public int ProcessId { get; set; }
    public ProcessActionEmailContactPaginationDto Process { get; set; }
}