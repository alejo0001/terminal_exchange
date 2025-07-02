using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Emails.Commands.SetActionTypeOnEmailSendingFailure;
using MassTransit;
using MediatR;
using NotificationAPI.Contracts.Commands;

namespace CrmAPI.Presentation.Consumers;

[UsedImplicitly]
public class CreateEmailFaultConsumer : IConsumer<Fault<CreateEmail>>
{
    private readonly ISender _sender;

    public CreateEmailFaultConsumer(ISender sender) => _sender = sender;

    public async Task Consume(ConsumeContext<Fault<CreateEmail>> context)
    {
        var command = new SetActionTypeOnEmailSendingFailureCommand(
            context.Message.Message.CorrelationId,
            context.Message.FaultMessageTypes);

        await _sender.Send(command, CancellationToken.None);
    }
}
