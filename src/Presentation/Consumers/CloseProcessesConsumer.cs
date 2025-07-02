using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Processes.Commands.CloseProcesses;
using CroupierAPI.Contracts.Commands;
using MassTransit;
using MediatR;

namespace CrmAPI.Presentation.Consumers;

public class CloseProcessesConsumer: IConsumer<CloseProcesses>
{
    private readonly ISender _mediator;
    private readonly IMapper _mapper;

    public CloseProcessesConsumer(ISender mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<CloseProcesses> context)
    {
        await _mediator.Send(_mapper.Map<CloseProcessesCommand>(context.Message), context.CancellationToken);
    }
}