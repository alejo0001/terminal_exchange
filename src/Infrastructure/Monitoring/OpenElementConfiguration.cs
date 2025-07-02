using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CrmAPI.Infrastructure.Monitoring;

public static class OpenElementConfiguration
{
    public static void SetupOpenTelemetry(this IServiceCollection services, ConfigurationManager configManager)
    {
        var moduleCode = configManager["ModuleCode"];
        var otelEndpointUrl = configManager["Monitoring:OTEL:Url"];

        var serviceName = $"api-{moduleCode}";
        var endpoint = new Uri(otelEndpointUrl);

        var telemetryBuilder = services.AddOpenTelemetry();

        telemetryBuilder.WithTracing(
            builder => builder
                .AddSource(serviceName)
                .ConfigureResource(resource => resource.AddService(serviceName))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(o => { o.Endpoint = endpoint; }));

        telemetryBuilder.WithMetrics(
            builder => builder
                .AddMeter(serviceName)
                .ConfigureResource(resource => resource.AddService(serviceName))
                .AddRuntimeInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddProcessInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEventCountersInstrumentation(
                    c =>
                    {
                        c.AddEventSources(
                            "Microsoft.AspNetCore.Hosting",
                            "Microsoft-AspNetCore-Server-Kestrel",
                            "System.Net.Http",
                            "System.Net.Sockets");
                    })
                .AddOtlpExporter(
                    o =>
                    {
                        o.Protocol = OtlpExportProtocol.Grpc;
                        o.Endpoint = endpoint;
                    }));
    }
}
