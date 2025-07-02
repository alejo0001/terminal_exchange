using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;

namespace CrmAPI.Application.Processes.Commands.DeleteProcess;

public class DeleteProcessCommand : IRequest
{
    public int Id { get; set; }
}
    
public class DeleteProcessCommandHandler : IRequestHandler<DeleteProcessCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteProcessCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
        
    public async Task<Unit> Handle(DeleteProcessCommand request, CancellationToken cancellationToken)
    {
        Process process = await _context.Processes.FindAsync(request.Id);
        if (process == null)
        {
            throw new NotFoundException(nameof(Process), request.Id);
        }
        process.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}