using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using CourseApi.Data;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Infrastructure.Persistence;
using CrmAPI.Presentation.Filters;
using CrmAPI.Presentation.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace CrmAPI.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(configuration);

        services.AddDatabaseDeveloperPageExceptionFilter();

        // This service most probably needs Transient/Scoped dependency, so cannot be Singleton anymore
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IAppSettingsService, AppSettingsService>();

        services.AddHttpContextAccessor();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        services.AddHealthChecks()
            .AddDbContextCheck<CourseApiContext>();

        services.AddControllers(options =>
                options.Filters.Add<ApiExceptionFilterAttribute>())
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

        // Customise default API behaviour
        services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder => builder
                .WithOrigins(configuration.GetSection("Settings:CorsAllowedDomains")?.Value.Split(";"))
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
        });

        services.AddOpenApiDocument(configure =>
        {
            configure.Title = "Intranet_Crm";
            configure.Version = "v1";
            configure.AddSecurity("oidc", Enumerable.Empty<string>(), new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.OAuth2,
                In = OpenApiSecurityApiKeyLocation.Header,
                OpenIdConnectUrl = $"{configuration["Jwt:Authority"]}/.well-known/openid-configuration",
                Flow = OpenApiOAuth2Flow.Application,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = $"{configuration["Jwt:Authority"]}/protocol/openid-connect/auth",
                        TokenUrl = $"{configuration["Jwt:Authority"]}/protocol/openid-connect/token",
                        Scopes = new Dictionary<string, string>{{"openid", "User Profile"}}
                    }
                }
            });

            configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("oidc"));
        });

        return services;
    }
}
