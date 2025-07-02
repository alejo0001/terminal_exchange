using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class CourseSpecialityDto : IMapFrom<CourseSpeciality>
{
    public int SpecialityId { get; set; }
    public int CourseId { get; set; }
    public SpecialityDto Speciality { get; set; }
}