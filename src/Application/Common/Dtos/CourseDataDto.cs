using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class CourseDataDto : IMapFrom<CourseData>
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string SeoTitle { get; set; }
}