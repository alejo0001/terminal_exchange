using CourseApi.Data.Domain;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Common.Interfaces;

public interface ICoursesUnDbContext
{
    DbSet<Country> Countries { get; set; }
    DbSet<Course> Courses { get; set; }
    DbSet<CourseArea> CourseAreas { get; set; }
    DbSet<Area> Areas { get; set; }
    DbSet<Speciality> Specialities { get; set; }
    DbSet<CountryAreaSpeciality> CountryAreaSpecialities { get; set; }
    DbSet<CountryArea> CountryAreas { get; set; }
    DbSet<AreaSpeciality> AreaSpecialities { get; set; }
    DbSet<Guarantor> Guarantors { get; set; }
    DbSet<CourseData> CourseData { get; set; }
    DbSet<CourseDataGuarantor> CourseDataGuarantors { get; set; }
    DbSet<CourseSpeciality> CourseSpecialities { get; set; }
    DbSet<CourseTypeCountry> CourseTypeCountries { get; set; }
    DbSet<CrmEnrollment> CrmEnrollments { get; set; }
}
