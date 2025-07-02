using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Emails.Commands.SendEmailCommercialAssignment;
using CrmAPI.Contracts.Commands;
using MassTransit;
using MediatR;

namespace CrmAPI.Presentation.Consumers;

[UsedImplicitly]
public class SendEmailCommercialAssignmentConsumer : IConsumer<SendEmailCommercialAssignment>
{
    private readonly ISender _sender;
    private readonly string _apiKey;

    public SendEmailCommercialAssignmentConsumer(ISender sender, IAppSettingsService appSettingsService)
    {
        _sender = sender;
        _apiKey = appSettingsService["Constants:ApiKey"];
    }

    public async Task Consume(ConsumeContext<SendEmailCommercialAssignment> context)
    {
        var command = new SendEmailCommercialAssignmentCommand
        {
            ProcessIds = context.Message.ProcessIds, 
            ApiKey = _apiKey
        };

        await _sender.Send(command);
    }
}