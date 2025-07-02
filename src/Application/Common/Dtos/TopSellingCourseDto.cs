using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class TopSellingCourseDto : IMapFrom<TopSellingCourse>
{
    public int FacultyId { get; set; }

    public string Title { get; set; }

    public string CourseCode { get; set; }
    
    public string CourseTypeBaseCode { get; set; }
        
    public int CourseCountryId { get; set; }

    public int CountryId { get; set; }

    public int Total { get; set; }
}