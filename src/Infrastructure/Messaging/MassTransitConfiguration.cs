using System;
using System.Reflection;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Infrastructure.Utilities;
using CroupierAPI.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationAPI.Contracts.Commands;

namespace CrmAPI.Infrastructure.Messaging;

public static class MassTransitConfiguration
{
    public static IServiceCollection RegisterMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        var endpointNameFormatter = DefaultEndpointNameFormatter.Instance;

        services.AddMassTransit(
            config =>
            {
                config.AddConsumers(Assembly.GetEntryAssembly());

                config.AddRequestClient<CreateContact>();
                config.AddRequestClient<ContactCroupierDto>();
                config.AddRequestClient<CreateProcess>();

                // Avoid communication with RabbitMQ while maldito NSWag is doing its thing.
                // Still, any "rider" registration is required, otherwise DI setup isn't complete and NSwag will fail.
                if (configuration.IsNSwagRunning())
                {
                    config.UsingInMemory();

                    return;
                }

                config.UsingRabbitMq(
                    (context, cfg) =>
                    {
                        cfg.Host(
                            configuration["RabbitMQ:HostName"],
                            configuration["RabbitMQ:VirtualHost"],
                            h =>
                            {
                                h.Username(configuration["RabbitMQ:Username"]);
                                h.Password(configuration["RabbitMQ:Password"]);
                            });
                        cfg.ConfigureEndpoints(context, endpointNameFormatter);
                    });
            });

        ConfigureEndpointConvention<CreateEmail>(endpointNameFormatter);

        return services;
    }

    private static void ConfigureEndpointConvention<TMessage>(IEndpointNameFormatter endpointNameFormatter)
        where TMessage : class =>
        EndpointConvention.Map<TMessage>(new Uri($"queue:{endpointNameFormatter.Message<TMessage>()}"));
}
