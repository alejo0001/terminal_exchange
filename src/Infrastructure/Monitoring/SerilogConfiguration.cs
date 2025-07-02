using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Templates;
using Serilog.Templates.Themes;

namespace CrmAPI.Infrastructure.Monitoring;

public static class SerilogConfiguration
{
    private static readonly ExpressionTemplate ConsoleFormatter;

    static SerilogConfiguration()
    {
        const string template = "[{@t:HH:mm:ss} {@l:u3}] [{SourceContext}]\n  {@m}\n{@x}";

        ConsoleFormatter = new(template, theme: TemplateTheme.Code);
    }

    /// <summary>
    ///     Sets up Serilog even before application composition phase so this phase will be logged from early on.
    /// </summary>
    public static void SetupBootstrapLogging()
    {
        SelfLog.Enable(msg => Debug.WriteLine(msg));

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Console(ConsoleFormatter)
            .CreateBootstrapLogger();

        Log.ForContext(typeof(SerilogConfiguration))
            .Information("We are online, starting to create the application ^_~");
    }

    public static void SetupSerilog(
        this ILoggingBuilder loggingBuilder,
        IHostBuilder hostBuilder,
        ConfigurationManager configManager,
        IHostEnvironment hostEnvironment)
    {
        var moduleCode = configManager["ModuleCode"];
        var elasticUrl = configManager["Monitoring:Elasticsearch:Url"];
        var elasticUserName = configManager["Monitoring:Elasticsearch:Username"];
        var elasticPassword = configManager["Monitoring:Elasticsearch:Password"];

        var envNameLower = hostEnvironment.EnvironmentName.ToLower();

        var serviceNameInOtel = $"api-{moduleCode}";

        var indexFormat = $"{moduleCode}-logs-{envNameLower.Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}";

        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(configManager)
            .MinimumLevel.Warning()
            .MinimumLevel.Override("CrmAPI", LogEventLevel.Information)
            .Enrich.WithAssemblyName()
            .Enrich.WithClientIp()
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .WriteTo.Console(ConsoleFormatter)
            .WriteTo.Elasticsearch(
                new(new Uri(elasticUrl!))
                {
                    IndexFormat = indexFormat,
                    AutoRegisterTemplate = true,
                    ModifyConnectionSettings = x => x.BasicAuthentication(elasticUserName, elasticPassword),
                })
            .WriteTo.OpenTelemetry(
                opts =>
                {
                    opts.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["app"] = serviceNameInOtel,
                        ["runtime"] = "dotnet",
                        ["service.name"] = serviceNameInOtel,
                    };
                });

#if INCLUDE_SEQ
        ConfigureSeq(configManager, loggerConfig);
#endif

        loggingBuilder.ClearProviders();

        Log.Logger = loggerConfig.CreateLogger();

        loggingBuilder.AddSerilog();

        hostBuilder.UseSerilog();
    }

#if INCLUDE_SEQ
    private static void ConfigureSeq(IConfiguration configuration, LoggerConfiguration loggerConfig)
    {
        var seqUrl = configuration["Monitoring:Seq:Url"];
        if (string.IsNullOrWhiteSpace(seqUrl))
        {
            return;
        }

        var seqApiKey = configuration["Monitoring:Seq:Url"] ?? string.Empty;

        loggerConfig.WriteTo.Seq(seqUrl, apiKey: seqApiKey);
    }
#endif
}
