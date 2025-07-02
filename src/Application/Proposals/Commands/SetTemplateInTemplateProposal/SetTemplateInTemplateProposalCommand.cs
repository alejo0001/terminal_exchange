using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Proposals.Commands.SetTemplateInTemplateProposal;

[Authorize(Roles = "Administrador")]
public class SetTemplateInTemplateProposalCommand: IRequest
{
    public int TemplateProposalId { get; set; }
    public int TemplateId { get; set; }
}


public class SetTemplateInTemplateProposalCommandHandler : IRequestHandler<SetTemplateInTemplateProposalCommand>
{    
    private readonly IApplicationDbContext _context;

    public SetTemplateInTemplateProposalCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(SetTemplateInTemplateProposalCommand request, CancellationToken cancellationToken)
    {

        var templateProposal = await _context.TemplateProposals
            .FirstOrDefaultAsync(tp => tp.Id == request.TemplateProposalId, cancellationToken);

        if (templateProposal is null)
        {
            throw new NotFoundException("TemplateProposal not found!");
        }
        
        var template = await _context.Templates
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

        if (template is null)
        {
            throw new NotFoundException("Template not found!");
        }

        var newTemplateProposalTemplate = new TemplateProposalTemplate()
        {
            TemplateProposalId = request.TemplateProposalId,
            TemplateProposal = templateProposal,
            TemplateId =  request.TemplateId,
            Template = template
        };

        _context.TemplateProposalTemplates.Add(newTemplateProposalTemplate);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value; 
    }
}