using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;

namespace CrmAPI.Application.Annotations.Commands.DeleteAnnotation;

public class DeleteAnnotationCommand : IRequest
{
    public int Id { get; set; }
}
    
public class DeleteAnnotationCommandHandler : IRequestHandler<DeleteAnnotationCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteAnnotationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
        
    public async Task<Unit> Handle(DeleteAnnotationCommand request, CancellationToken cancellationToken)
    {
        Annotation annotation = await _context.Annotations.FindAsync(request.Id);
        if (annotation == null)
        {
            throw new NotFoundException(nameof(Annotation), request.Id);
        }
        if (annotation.Mandatory)
        {
            throw new ForbiddenAccessException();
        }
        annotation.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}