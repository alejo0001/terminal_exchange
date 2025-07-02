using System.Collections.Generic;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class CourseDto : IMapFrom<Course>
{
    public int Id { get; set; }
    public int OriginalCourseId { get; set; }
    public int? CourseTypeId { get; set; }
    public CourseTypeDto CourseType { get; set; }
    public string Title { get; set; }
    public string Code { get; set; }
    public GuarantorDto Guarantor { get; set; }
    public List<CourseFacultyDto> CourseFaculties { get; set; }
    public List<CourseSpecialityDto> CourseSpecialities { get; set; }
}