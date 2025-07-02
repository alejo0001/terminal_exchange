using System;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Infrastructure.ApiClients;
using CrmAPI.Infrastructure.Configuration;
using CrmAPI.Infrastructure.Files;
using CrmAPI.Infrastructure.Messaging;
using CrmAPI.Infrastructure.Monitoring;
using CrmAPI.Infrastructure.Persistence;
using CrmAPI.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CrmAPI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        ConfigurationManager configManager,
        IHostEnvironment hostEnvironment,
        IHostBuilder hostBuilder,
        ILoggingBuilder loggingBuilder)
    {
        configManager.SetupAppConfiguration(hostEnvironment);

        services.AddMonitoring(configManager, loggingBuilder, hostEnvironment, hostBuilder);

        // This approach is beneficial for all the application, but wrapper and this patterns usage
        // exists only in Infrastructure layer as of now, so registering it here.
        services.AddTransient(typeof(Lazy<>), typeof(LazyDependencyWrapper<>));

        services.RegisterPersistence(configManager);

        services.RegisterApiClients();

        services.RegisterMassTransit(configManager);

        services.AddMemoryCacheWithCompacter<MemoryCacheCompacter>();

        services.AddScoped<IDomainEventService, DomainEventService>();

        services.AddScoped<IFileService, FileService>();

        services.AddSingleton<IDateTime, DateTimeService>();

        services.AddTransient<IBlobStorageService, BlobStorageService>();

        services.AddScoped<IWorkingDays, WorkingDaysService>();

        services.AddScoped<ICalendarService, CalendarService>();

        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IIsDefaultService, IsDefaultService>();

        services.AddScoped<IOrganizationNodeExplorerService, OrganizationNodeExplorerService>();

        services.AddSingleton<IServiceBusService, ServiceBusService>();

        services.AddScoped<IProcessesService, ProcessesService>();

        services.AddScoped<ITranslationsService, TranslationsService>();

        services.AddScoped<IActionsService, ActionsService>();

        services.AddScoped<IEmployeeService, EmployeeService>();

        services.AddScoped<IPotentialsService, PotentialsService>();

        services.AddScoped<IOrderImportedService, OrderImportedService>();

        services.AddScoped<IEntityClonerService<IApplicationDbContext>, EntityClonerService<IApplicationDbContext>>();

        services.AddSingleton<IEFCoreFunctions, EFCoreFunctions>();

        services.AddSingleton<IEmailSendTestingFeatureFlagService, EmailSendTestingFeatureFlagService>();

        services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(cfg =>
            {
                //AUTHORITY
                cfg.Authority = configManager["Jwt:Authority"];
                cfg.Audience = configManager["Jwt:Audience"];
                cfg.IncludeErrorDetails = true;
                cfg.RequireHttpsMetadata = false;
                cfg.TokenValidationParameters = new()
                {
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    //ISSUER
                    ValidIssuer = configManager["Jwt:Authority"],
                    ValidateLifetime = true,
                };

                cfg.Events = new()
                {
                    OnAuthenticationFailed = c =>
                    {
                        c.NoResult();
                        c.Response.StatusCode = 401;
                        c.Response.ContentType = "text/plain";
                        return c.Response.WriteAsync(c.Exception.ToString());
                    },
                };
            });

        return services;
    }

    /// <summary>
    ///     Monitoring and logging related configuration bootstrapping.
    /// </summary>
    private static void AddMonitoring(
        this IServiceCollection services,
        ConfigurationManager configManager,
        ILoggingBuilder loggingBuilder,
        IHostEnvironment hostEnvironment,
        IHostBuilder hostBuilder)
    {
        loggingBuilder.SetupSerilog(hostBuilder, configManager, hostEnvironment);
        services.SetupOpenTelemetry(configManager);
    }

    /// <summary>
    ///     Enforces to register compacter service too, as .NET MemoryCache do not have any automatic means for this.
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="TCompacterService"></typeparam>
    private static void AddMemoryCacheWithCompacter<TCompacterService>(this IServiceCollection services)
        where TCompacterService : class, IHostedService
    {
        services.AddMemoryCache(opt =>
        {
            opt.CompactionPercentage = 0.10;
            opt.ExpirationScanFrequency = TimeSpan.FromMinutes(2);
            opt.TrackStatistics = true;
        });

        services.AddHostedService<TCompacterService>();
    }
}
