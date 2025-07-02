using CrmAPI.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Infrastructure.Persistence;

public class CoursesFpDbContext : BaseCoursesDbContext, ICoursesFpDbContext, IUnitOfWork<ICoursesFpDbContext>
{
    /// <inheritdoc />
    public CoursesFpDbContext(DbContextOptions<CoursesFpDbContext> options) : base(options) { }
}
