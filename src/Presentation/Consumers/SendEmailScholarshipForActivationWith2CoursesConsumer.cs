using System;
using System.Threading.Tasks;
using CrmAPI.Application.Emails.Commands.SendEmailScholarshipForActivationWith2Courses;
using CrmAPI.Contracts.Commands;
using MassTransit;
using MediatR;

namespace CrmAPI.Presentation.Consumers;

[UsedImplicitly]
public sealed class SendEmailScholarshipForActivationWith2CoursesConsumer
    : IConsumer<ISendEmailScholarshipForActivationWith2Courses>
{
    private readonly ISender _sender;

    public SendEmailScholarshipForActivationWith2CoursesConsumer(ISender sender) => _sender = sender;

    public async Task Consume(ConsumeContext<ISendEmailScholarshipForActivationWith2Courses> context)
    {
        var command = new SendEmailScholarshipForActivationWith2CoursesCommand(context.Message.Dto);

        var result = await _sender.Send(command, context.CancellationToken);

        // TODO: Think about error result handling, it's smelly.

        if (context.RequestId.GetValueOrDefault() == Guid.Empty)
        {
            return;
        }

        // TODO: Think about error result handling, it's smelly.
        await result.SwitchAsync(
            val => context.RespondAsync(val),
            context.RespondAsync);
    }

    [UsedImplicitly]
    public class Definition : ConsumerDefinition<SendEmailScholarshipForActivationWith2CoursesConsumer>
    {
        public Definition() =>
            EndpointName = DefaultEndpointNameFormatter.Instance
                .Message<ISendEmailScholarshipForActivationWith2Courses>();
    }
}
