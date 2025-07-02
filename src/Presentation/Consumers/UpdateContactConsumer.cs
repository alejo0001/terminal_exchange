using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Annotations.Commands.CreateAnnotation;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.ContactLeads.Commands.CreateOrUpdateContactLead;
using CrmAPI.Application.Contacts.Commands.UpdateContact;
using CroupierAPI.Contracts.Commands;
using MassTransit;
using MediatR;

namespace CrmAPI.Presentation.Consumers;

public class UpdateContactConsumer : IConsumer<UpdateContact>
{
    private readonly ISender _mediator;
    private readonly IMapper _mapper;

    public UpdateContactConsumer(ISender mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }
    
    public async Task Consume(ConsumeContext<UpdateContact> context)
    {
       
        await _mediator.Send(_mapper.Map<UpdateContactCommand>(context.Message));
        if (context.Message.AnnotationCreateDto is not null)
        {
            await _mediator.Send(new CreateAnnotationCommand
            {
                ContactId = context.Message.AnnotationCreateDto.ContactId,
                Comment = context.Message.AnnotationCreateDto.Comment,
                Title = context.Message.AnnotationCreateDto.Title,
                LastEditor = context.Message.AnnotationCreateDto.LastEditor,
                Mandatory = context.Message.AnnotationCreateDto.Mandatory
            });    
        }


        await _mediator.Send(_mapper.Map<CreateOrUpdateContactLeadCommand>(context.Message.ContactLeadDto));
    }
}