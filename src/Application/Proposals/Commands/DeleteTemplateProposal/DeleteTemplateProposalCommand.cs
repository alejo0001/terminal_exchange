using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Proposals.Commands.DeleteTemplateProposal;

[Authorize(Roles = "Administrador")]
public class DeleteTemplateProposalCommand: IRequest
{
    public int Id { get; set; }
}

public class DeleteTemplateProposalCommandHandler : IRequestHandler<DeleteTemplateProposalCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteTemplateProposalCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteTemplateProposalCommand request, CancellationToken cancellationToken)
    {
        var templateProposal = await _context.TemplateProposals.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (templateProposal == null)
        {
            throw new NotFoundException(nameof(templateProposal), request.Id);
        }

        templateProposal.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}