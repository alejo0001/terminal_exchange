using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Emails.Commands.SendEmailRecords2ScholarshipActivation;
using CrmAPI.Contracts.Commands;
using MassTransit;
using MediatR;

namespace CrmAPI.Presentation.Consumers;

[UsedImplicitly]
public class SendEmailRecords2ScholarshipActivationConsumer : IConsumer<SendEmailRecords2ScholarshipActivation>
{
    private readonly ISender _sender;
    private readonly string _apiKey;

    public SendEmailRecords2ScholarshipActivationConsumer(ISender sender, IAppSettingsService appSettingsService)
    {
        _sender = sender;
        _apiKey = appSettingsService["Constants:ApiKey"];
    }

    public async Task Consume(ConsumeContext<SendEmailRecords2ScholarshipActivation> context)
    {
        var command = new SendEmailRecords2ScholarshipActivationCommand
        {
            ProcessIds = context.Message.ProcessIds,
            ApiKey = _apiKey
        };

        await _sender.Send(command);
    }
}
