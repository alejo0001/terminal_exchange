using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Proposals.Commands.DeleteTemplate;

[Authorize(Roles = "Administrador")]
public class DeleteTemplateCommand: IRequest
{
    public int Id { get; set; }
}

public class DeleteTemplateCommandHandler : IRequestHandler<DeleteTemplateCommand>
{    
    
    private readonly IApplicationDbContext _context;

    public DeleteTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
    {
        var templateProposal = await _context.Templates.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (templateProposal == null)
        {
            throw new NotFoundException(nameof(templateProposal), request.Id);
        }

        templateProposal.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}