using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Proposals.Commands.DeleteTemplateInTemplateProposal;


[Authorize(Roles = "Administrador")]
public class DeleteTemplateInTemplateProposalCommand: IRequest
{
    public int TemplateProposalId { get; set; }
    public int TemplateId { get; set; }
}

public class DeleteTemplateInTemplateProposalCommandHandler : IRequestHandler<DeleteTemplateInTemplateProposalCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteTemplateInTemplateProposalCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteTemplateInTemplateProposalCommand request, CancellationToken cancellationToken)
    {
        var templateProposalTemplates = await _context.TemplateProposalTemplates
            .Where(t => t.TemplateProposalId == request.TemplateProposalId
                        && t.TemplateId == request.TemplateId)
            .ToListAsync( cancellationToken);

        if (templateProposalTemplates is null)
        {
            throw new NotFoundException("templateProposalTemplate not found!");
        }

        templateProposalTemplates.ForEach(tpt => tpt.IsDeleted = true);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Unit.Value; 
    }
}