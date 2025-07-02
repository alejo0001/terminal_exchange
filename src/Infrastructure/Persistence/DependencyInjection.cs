using System;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CrmAPI.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection RegisterPersistence(
        this IServiceCollection services,
        ConfigurationManager configManager)
    {
        services.AddOptions<ConnectionStringsSettings>()
            .BindConfiguration(ConnectionStringsSettings.SectionName);

        if (configManager.GetValue<bool>("UseInMemoryDatabase"))
        {
            // Didn't change this logic just yet in terms of Key Vault implementation, because seems to be not in use...
            services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(configManager["Database:InMemory"]));

            services.AddDbContext<ILeadsDbContext, LeadsDbContext>(options =>
                options.UseInMemoryDatabase(configManager["Database:InMemoryLeads"]));

            services.AddDbContext<ITlmkDbContext, TlmkDbContext>(options =>
                options.UseInMemoryDatabase(configManager["Database:InMemoryTlmk"]));
        }
        else
        {
            services.AddDbContext<IApplicationDbContext, ApplicationDbContext>((sp, options) =>
            {
                options.UseSqlServer(
                    sp.GetRequiredService<IOptionsSnapshot<ConnectionStringsSettings>>().Value.Intranet);
                #if (debug)
                options.EnableSensitiveDataLogging();
                #endif
            });

            services.AddDbContext<ICoursesUnDbContext, CoursesUnDbContext>((sp, options) =>
                options.UseSqlServer(
                    sp.GetRequiredService<IOptionsSnapshot<ConnectionStringsSettings>>().Value.WebDatabase));

            services.AddDbContext<ICoursesFpDbContext, CoursesFpDbContext>((sp, options) =>
                options.UseSqlServer(
                    sp.GetRequiredService<IOptionsSnapshot<ConnectionStringsSettings>>().Value.WebFpDatabase));

            services.AddDbContext<ILeadsDbContext, LeadsDbContext>((sp, options) =>
                options.UseMySql(
                    sp.GetRequiredService<IOptionsSnapshot<ConnectionStringsSettings>>().Value.Leads,
                    new MySqlServerVersion(new Version(8, 0, 21))));

            services.AddDbContext<ITlmkDbContext, TlmkDbContext>((sp, options) =>
                options.UseMySql(
                    sp.GetRequiredService<IOptionsSnapshot<ConnectionStringsSettings>>().Value.TLMK,
                    new MySqlServerVersion(new Version(8, 0, 21))));
        }

        services.AddScoped<IUnitOfWork<ICoursesUnDbContext>>(
            sp => (sp.GetRequiredService<ICoursesUnDbContext>() as CoursesUnDbContext)!);

        services.AddScoped<IUnitOfWork<ICoursesFpDbContext>>(
            sp => (sp.GetRequiredService<ICoursesFpDbContext>() as CoursesFpDbContext)!);

        services.AddScoped<IUnitOfWork<IApplicationDbContext>>(
            sp => (sp.GetRequiredService<IApplicationDbContext>() as ApplicationDbContext)!);

        services.AddTransient<ApplicationDbContextSeed>();

        return services;
    }
}
