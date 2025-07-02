using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Proposals.Commands.CreateTemplateProposal;

[Authorize(Roles = "Administrador")]
public class CreateTemplateProposalCommand: TemplateProposalCreateDto, IRequest<int>
{
}

public class CreateTemplateProposalCommandHandler : IRequestHandler<CreateTemplateProposalCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateTemplateProposalCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<int> Handle(CreateTemplateProposalCommand request, CancellationToken cancellationToken)
    {
        var newTemplateProposal = await MapperTemplateProposal(request, cancellationToken);
        _context.TemplateProposals.Add(newTemplateProposal);
        await _context.SaveChangesAsync(cancellationToken);
        return newTemplateProposal.Id;
    }


    // TODO: hacer esto de la forma correcta con automapper
    private async Task<TemplateProposal> MapperTemplateProposal(CreateTemplateProposalCommand request, CancellationToken cancellationToken)
    {
        var tag =  await _context.Tags.FirstOrDefaultAsync(t => t.Id == request.TagId, cancellationToken);

        if (tag is null)
        {
            throw new NotFoundException("Tag not found!");
        }

        var templateProposal =  new TemplateProposal()
        {
            Name = request.Name,
            ProcessType = request.ProcessType,
            Day = request.Day,
            Attempt = request.Attempt,
            Colour = request.Colour,
            CourseKnown = request.CourseKnown,
            CourseTypeId = request.CourseTypeId ?? null,
            HasToSendEmail = request.HasToSendEmail,
            HasToSendWhatsApp = request.HasToSendWhatsApp,
            TagId = request.TagId,
            Tag = tag
        };
        
        if (request.CourseTypeId is not null)
        {
            var courseType = await _context.CourseTypes.FirstOrDefaultAsync(c => c.Id == request.CourseTypeId, cancellationToken);
            
            if (courseType is not null)
            {
                templateProposal.CourseType = courseType;
            }
        }


        return templateProposal;
    }
}