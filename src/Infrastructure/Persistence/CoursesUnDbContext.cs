using CourseApi.Data.Domain;
using CrmAPI.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Infrastructure.Persistence;

public class CoursesUnDbContext : BaseCoursesDbContext, ICoursesUnDbContext, IUnitOfWork<ICoursesUnDbContext>
{
    /// <inheritdoc />
    public CoursesUnDbContext(DbContextOptions<CoursesUnDbContext> options) : base(options) { }
}
