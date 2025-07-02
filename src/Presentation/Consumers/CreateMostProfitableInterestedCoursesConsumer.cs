using System;
using System.Threading.Tasks;
using CrmAPI.Application.ContactLeads.Commands.CreateMostProfitableInterestedCourses;
using CrmAPI.Contracts.Commands;
using MassTransit;
using MediatR;

namespace CrmAPI.Presentation.Consumers;

[UsedImplicitly]
public sealed class CreateMostProfitableInterestedCoursesConsumer : IConsumer<ICreateMostProfitableInterestedCourses>
{
    private readonly ISender _sender;

    public CreateMostProfitableInterestedCoursesConsumer(ISender sender) => _sender = sender;

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<ICreateMostProfitableInterestedCourses> context)
    {
        var command = new CreateMostProfitableInterestedCoursesCommand(context.Message.Dto);

        var result = await _sender.Send(command, context.CancellationToken);

        // TODO: Think about error result handling, it's smelly.

        if (context.RequestId.GetValueOrDefault() == Guid.Empty)
        {
            return;
        }

        // TODO: Think about error result handling, it's smelly.
        await result.SwitchAsync(
            context.RespondAsync,
            context.RespondAsync);
    }

    [UsedImplicitly]
    public class Definition : ConsumerDefinition<CreateMostProfitableInterestedCoursesConsumer>
    {
        public Definition() =>
            EndpointName = DefaultEndpointNameFormatter.Instance.Message<ICreateMostProfitableInterestedCourses>();
    }
}
