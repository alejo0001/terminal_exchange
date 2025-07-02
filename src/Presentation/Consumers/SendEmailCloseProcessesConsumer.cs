using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Emails.Commands.SendEmailCloseProcesses;
using CrmAPI.Contracts.Commands;
using MassTransit;
using MediatR;

namespace CrmAPI.Presentation.Consumers;

[UsedImplicitly]
public class SendEmailCloseProcessesConsumer : IConsumer<SendEmailCloseProcesses>
{
    private readonly ISender _sender;
    private readonly string _apiKey;
    
    public SendEmailCloseProcessesConsumer(ISender sender, IAppSettingsService appSettingsService)
    {
        _sender = sender;
        _apiKey = appSettingsService["Constants:ApiKey"];
    }
    
    public async Task Consume(ConsumeContext<SendEmailCloseProcesses> context)
    {
        var command = new SendEmailCloseProcessesCommand()
        {
            ProcessIds = context.Message.ProcessIds, 
            ApiKey = _apiKey
        };

        await _sender.Send(command);
    }
    
}