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

namespace CrmAPI.Application.Proposals.Commands.UpdateTemplateProposal;

[Authorize(Roles = "Administrador")]
public class UpdateTemplateProposalCommand: TemplateProposalUpdateDto, IRequest
{
}

public class UpdateTemplateProposalCommandHandler : IRequestHandler<UpdateTemplateProposalCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateTemplateProposalCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }


    public async Task<Unit> Handle(UpdateTemplateProposalCommand request, CancellationToken cancellationToken)
    {
        var templateProposal = await _context.TemplateProposals.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (templateProposal == null)
        {
            throw new NotFoundException(nameof(templateProposal), request.Id);
        }

        await UpdateTemplateProposal(templateProposal, request, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task UpdateTemplateProposal(TemplateProposal templateProposal, UpdateTemplateProposalCommand request, CancellationToken cancellationToken)
    {
        templateProposal.Name = request.Name;
        templateProposal.ProcessType = request.ProcessType;
        templateProposal.Day = request.Day;
        templateProposal.Attempt = request.Attempt;
        templateProposal.Colour = request.Colour;
        templateProposal.CourseKnown = request.CourseKnown;
        templateProposal.CourseTypeId = request.CourseTypeId;
        templateProposal.HasToSendEmail = request.HasToSendEmail;
        templateProposal.HasToSendWhatsApp = request.HasToSendWhatsApp;
        
        var tag =  await _context.Tags.FirstOrDefaultAsync(t => t.Id == request.TagId, cancellationToken);

        if (tag is null)
        {
            throw new NotFoundException("Tag not found!");
        }

        templateProposal.TagId = request.TagId;
        templateProposal.Tag = tag;
        templateProposal.CourseTypeId = request.CourseTypeId ?? null;
        
        if (request.CourseTypeId is not null)
        {
            var courseType = await _context.CourseTypes.FirstOrDefaultAsync(c => c.Id == request.CourseTypeId, cancellationToken);
            
            if (courseType is not null)
            {
                templateProposal.CourseType = courseType;
            }
        }
    }
}