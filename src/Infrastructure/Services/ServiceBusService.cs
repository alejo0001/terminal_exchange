using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using Newtonsoft.Json;

namespace CrmAPI.Infrastructure.Services;

public class ServiceBusService : IServiceBusService
{
    private readonly ServiceBusSender _serviceBusSender;

    public ServiceBusService(IAppSettingsService appSettingsService)
    {

        ServiceBusClient serviceBusClient = new(appSettingsService["ServiceBus:Default"]);
        _serviceBusSender = serviceBusClient.CreateSender(appSettingsService["ServiceBus:QueueName"]);
    }
        
    public async Task AddEventToQueue(EventDto eventCal)
    {
        var batch = await _serviceBusSender.CreateMessageBatchAsync();
            
        if (!batch.TryAddMessage(new ServiceBusMessage(JsonConvert.SerializeObject(eventCal))))
        {
            throw new Exception($"The notification {eventCal} is too large to fit in the batch.");
        }
            
        await _serviceBusSender.SendMessagesAsync(batch);
    }
 
}