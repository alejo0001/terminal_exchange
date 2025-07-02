using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;

namespace CrmAPI.Application.Annotations.Commands.UpdateAnnotation;

public class UpdateAnnotationCommand: AnnotationUpdateDto, IRequest
{
}
    
public class UpdateAnnotationCommandHandler : IRequestHandler<UpdateAnnotationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateAnnotationCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }
        
    public async Task<Unit> Handle(UpdateAnnotationCommand request, CancellationToken cancellationToken)
    {
        Annotation annotation = await _context.Annotations.FindAsync(request.Id);
        if (annotation == null)
        {
            throw new NotFoundException(nameof(Annotation), request.Id);
        }
        if (request.Mandatory || annotation.Mandatory)
        {
            throw new ForbiddenAccessException();
        }
        annotation.LastEditor = _currentUserService.Name;
        annotation.Title = request.Title;
        annotation.Comment = request.Comment;
        annotation.IsPrivate = request.IsPrivate;
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}