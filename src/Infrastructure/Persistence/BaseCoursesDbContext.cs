using System;
using System.Threading;
using System.Threading.Tasks;
using CourseApi.Data;
using CourseApi.Data.Domain;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Infrastructure.Persistence;

public abstract class BaseCoursesDbContext : DbContext
{
    protected BaseCoursesDbContext(DbContextOptions options) : base(options) { }

    public DbSet<Country> Countries { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<CourseArea> CourseAreas { get; set; }
    public DbSet<Area> Areas { get; set; }
    public DbSet<Speciality> Specialities { get; set; }
    public DbSet<CountryAreaSpeciality> CountryAreaSpecialities { get; set; }
    public DbSet<CountryArea> CountryAreas { get; set; }
    public DbSet<AreaSpeciality> AreaSpecialities { get; set; }
    public DbSet<Guarantor> Guarantors { get; set; }
    public DbSet<CourseData> CourseData { get; set; }
    public DbSet<CourseDataGuarantor> CourseDataGuarantors { get; set; }
    public DbSet<CourseSpeciality> CourseSpecialities { get; set; }
    public DbSet<CourseTypeCountry> CourseTypeCountries { get; set; }
    public DbSet<CrmEnrollment> CrmEnrollments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CourseApiContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        SaveChangesAsync(string.Empty, ct);

    public async Task<int> SaveChangesAsync(string _, CancellationToken ct)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreationDate = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.ModificationDate = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(ct);
    }
}
