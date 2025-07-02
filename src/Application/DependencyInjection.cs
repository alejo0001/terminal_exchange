using System;
using System.Linq;
using System.Reflection;
using CrmAPI.Application.Common.Behaviours;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Validation;
using CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses.Services;
using CrmAPI.Application.Contacts.Commands.RecoverContactActivations;
using CrmAPI.Application.Contacts.Services;
using CrmAPI.Application.Emails.Services;
using CrmAPI.Application.Processes.Services;
using CrmAPI.Application.Settings;
using CrmAPI.Application.UtcTimeZone.Services;
using CrmAPI.Application.WebEnrollments.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CrmAPI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped(typeof(ContactExistsByEmailValidator<>));
        services.AddMediatR(Assembly.GetExecutingAssembly());

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));

        ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Stop;
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

        services.ConfigurePopulateInterestedCourseFeature();

        services.AddOptions<DataBlobAuditoryConnectionSettings>()
            .BindConfiguration(DataBlobAuditoryConnectionSettings.SectionName);

        services.AddScoped<ICourseWebEnrollmentService, CourseWebEnrollmentService>();
        services.AddScoped<IContactsConsolidatorService, ContactsConsolidatorService>();
        services.AddScoped<ICommercialAssignmentService, CommercialAssignmentService>();
        services.AddScoped<ICloseProcessService, CloseProcessService>();
        services.AddScoped<ICrmMailerService, CrmMailerService>();

        services.AddSingleton<UtcTimeZoneService>();
        services.AddScoped<IRecoverContactActivationsCommand, RecoverContactActivationsCommandHandler>();


        return services;
    }

    private static IServiceCollection ConfigurePopulateInterestedCourseFeature(this IServiceCollection services)
    {
        services.AddOptions<TopSellerCourseSettings>()
            .Configure(settings =>
            {
                // Default config, used, if appsettings files don't define any of them.
                settings.SlidingExpirationPeriod = TimeSpan.FromHours(1);
                settings.AbsoluteExpirationPeriod = TimeSpan.FromHours(4);

                settings.StatisticsPeriod = TimeSpan.FromDays(365);

                settings.StatisticsTakeLimit = 10;
            })
            .PostConfigure(settings =>
            {
                // This is for cleanup, as collection type values can originate from various sources and thus duplicate.
                // Lowercase decision is based on the knowledge, how these values are in DB.
                settings.ExcludedPaymentTypes = settings.ExcludedPaymentTypes
                    .Select(x => x.ToLowerInvariant())
                    .Distinct()
                    .ToArray();
            })
            .BindConfiguration(TopSellerCourseSettings.SectionName);

        services.AddOptions<InterestedCoursePopulatorSettings>()
            .Configure(settings =>
            {
                // Should be relatively fail-safe values.
                settings.ContactsQueryMaxPageSize = 1000;
                settings.EntityCreationMaxChunkSize = 100;
            })
            .PostConfigure(settings =>
            {
                settings.ExcludedFaculties = settings.ExcludedFaculties
                    .Select(x => x.ToUpperInvariant())
                    .Distinct()
                    .ToArray();
            })
            .BindConfiguration(InterestedCoursePopulatorSettings.SectionName);

        services.AddScoped<IContactLeadsAnalyzerService, ContactLeadsAnalyzerService>();
        services.AddScoped<ITopSellerCourseService, TopSellerCourseService>();
        services.AddScoped<IWorkScheduleService, WorkScheduleService>();

        return services;
    }
}
