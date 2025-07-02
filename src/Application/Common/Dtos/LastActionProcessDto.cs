using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class LastActionProcessDto : IMapFrom<Action>
{
    public string Type { get; set; }
    public string CourseTitle  { get; set; }
}