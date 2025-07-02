using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Contacts.Queries.GetContactByEmailOrPhone;
using CroupierAPI.Contracts.Commands;
using MassTransit;
using MediatR;

namespace CrmAPI.Presentation.Consumers;

public class GetContactConsumer: IConsumer<GetContact>
{
    private readonly ISender _mediator;
    private readonly IMapper _mapper;
    
    public GetContactConsumer(ISender mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }
    
    public async Task Consume(ConsumeContext<GetContact> context)
    {
        var response = await _mediator.Send(new GetContactByEmailOrPhoneQuery
        {
            Data = context.Message
        });
        await context.RespondAsync(response);
    }
}