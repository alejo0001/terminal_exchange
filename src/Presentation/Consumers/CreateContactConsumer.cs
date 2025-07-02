using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Contacts.Commands.CreateContact;
using CroupierAPI.Contracts.Commands;
using MassTransit;
using MediatR;

namespace CrmAPI.Presentation.Consumers;

public class CreateContactConsumer: IConsumer<CreateContact>
{
    private readonly ISender _mediator;
    private readonly IMapper _mapper;

    public CreateContactConsumer(ISender mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<CreateContact> context)
    {
        var response = await _mediator.Send(_mapper.Map<CreateContactCommand>(context.Message));
        await context.RespondAsync(response);
    }
}