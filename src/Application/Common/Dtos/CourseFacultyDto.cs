using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class CourseFacultyDto : IMapFrom<CourseFaculty>
{
    public int FacultyId { get; set; }

    public FacultyDto Faculty { get; set; }

    public int CourseId { get; set; }
}