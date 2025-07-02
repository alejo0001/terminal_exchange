using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Infrastructure.ApiClients.RequestHandlers;
using CrmAPI.Infrastructure.Settings;
using CrmAPI.Infrastructure.Settings.Validation;
using FluentValidation;
using IL.FluentValidation.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CrmAPI.Infrastructure.ApiClients;

public static class DependencyInjection
{
    public static IServiceCollection RegisterApiClients(this IServiceCollection services) =>
        services.AddApiOptions().AddClients();

    private static IServiceCollection AddApiOptions(this IServiceCollection services)
    {
        services.AddTransient<IValidator<AuthApiSettings>, AuthApiSettingsValidator>() // Damn NSwag!
            .AddOptions<AuthApiSettings>()
            .BindConfiguration(AuthApiSettings.SectionName)
            .ValidateOnStart()
            .ValidateWithFluentValidator();

        services.AddTransient<IValidator<CourseUnApiSettings>, CourseUnApiSettingsValidator>()
            .AddOptions<CourseUnApiSettings>()
            .BindConfiguration(CourseUnApiSettings.SectionName)
            .ValidateOnStart()
            .ValidateWithFluentValidator();
        
        services.AddTransient<IValidator<ManagementApiSettings>, ManagementApiSettingsValidator>()
            .AddOptions<ManagementApiSettings>()
            .BindConfiguration(ManagementApiSettings.SectionName)
            .ValidateOnStart()
            .ValidateWithFluentValidator();

        services.AddTransient<IValidator<CourseFPApiSettings>, CourseFPApiSettingsValidator>()
            .AddOptions<CourseFPApiSettings>()
            .BindConfiguration(CourseFPApiSettings.SectionName)
            .ValidateOnStart()
            .ValidateWithFluentValidator();

        services.AddTransient<IValidator<CroupierApiSettings>, CroupierApiSettingsValidator>()
            .AddOptions<CroupierApiSettings>()
            .BindConfiguration(CroupierApiSettings.SectionName)
            .ValidateOnStart()
            .ValidateWithFluentValidator();

        services.AddTransient<IValidator<TranslationApiSettings>, TranslationApiSettingsValidator>()
            .AddOptions<TranslationApiSettings>()
            .BindConfiguration(TranslationApiSettings.SectionName)
            .ValidateOnStart()
            .ValidateWithFluentValidator();

        services.AddTransient<IValidator<HrApiSettings>, HrApiSettingsValidator>()
            .AddOptions<HrApiSettings>()
            .BindConfiguration(HrApiSettings.SectionName)
            .ValidateOnStart()
            .ValidateWithFluentValidator();

        services.AddTransient<IValidator<CalendarApiSettings>, CalendarApiSettingsValidator>()
            .AddOptions<CalendarApiSettings>()
            .BindConfiguration(CalendarApiSettings.SectionName)
            .ValidateOnStart()
            .ValidateWithFluentValidator();

        return services;
    }

    private static IServiceCollection AddClients(this IServiceCollection services)
    {
        services.AddTransient<AuthHeaderHandler>();

        services.AddHttpClient<IAuthApiClient, AuthApiClient>((sp, client) =>
                client.BaseAddress = new(
                    sp.GetRequiredService<IOptionsMonitor<AuthApiSettings>>().CurrentValue.AuthUrl))
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddHttpClient<ICourseUnApiClient, CourseUnApiClient>((sp, client) =>
            client.BaseAddress = new(
                sp.GetRequiredService<IOptionsMonitor<CourseUnApiSettings>>().CurrentValue.Enrollment));
        
        services.AddHttpClient<IManagementApiClient, ManagementApiClient>((sp, client) =>
            client.BaseAddress = new(
                sp.GetRequiredService<IOptionsMonitor<ManagementApiSettings>>().CurrentValue.Enrollment));

        services.AddHttpClient<ICourseFPApiClient, CourseFPApiClient>((sp, client) =>
            client.BaseAddress = new(
                sp.GetRequiredService<IOptionsMonitor<CourseFPApiSettings>>().CurrentValue.Enrollment));

        services.AddHttpClient<ICroupierApiClient, CroupierApiClient>((sp, client) =>
            client.BaseAddress = new(
                sp.GetRequiredService<IOptionsMonitor<CroupierApiSettings>>().CurrentValue.Endpoint));

        services.AddHttpClient<ITranslationApiClient, TranslationApiClient>((sp, client) =>
                client.BaseAddress = new(
                    sp.GetRequiredService<IOptionsMonitor<TranslationApiSettings>>().CurrentValue.TranslationsUrl))
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddHttpClient<IHrApiClient, HrApiClient>((sp, client) =>
                client.BaseAddress = new(
                    sp.GetRequiredService<IOptionsMonitor<HrApiSettings>>().CurrentValue.Endpoint))
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddHttpClient<ICalendarApiClient, CalendarApiClient>((sp, client) =>
                client.BaseAddress = new(
                    sp.GetRequiredService<IOptionsMonitor<CalendarApiSettings>>().CurrentValue.Endpoint))
            .AddHttpMessageHandler<AuthHeaderHandler>();

        return services;
    }
}
