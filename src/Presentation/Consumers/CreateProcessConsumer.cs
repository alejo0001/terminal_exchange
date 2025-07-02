using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Processes.Commands.CreateProcess;
using CroupierAPI.Contracts.Commands;
using CroupierAPI.Contracts.Events;
using MassTransit;
using MediatR;

namespace CrmAPI.Presentation.Consumers;

public class CreateProcessConsumer: IConsumer<CreateProcess>
{
    private readonly ISender _mediator;
    private readonly IMapper _mapper;

    public CreateProcessConsumer(ISender mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }
    
    public async Task Consume(ConsumeContext<CreateProcess> context)
    {
        var response = await _mediator.Send(_mapper.Map<CreateProcessCommand>(context.Message));
        await context.RespondAsync(new ProcessCreated
        {
            Id = response,
            CorrelationId = context.Message.CorrelationId,
        });
    }
}