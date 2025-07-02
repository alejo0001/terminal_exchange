using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Annotations.Commands.CreateAnnotation;

public class CreateAnnotationCommand: AnnotationCreateDto, IRequest<int>
{
}
    
public class CreateAnnotationCommandHandler : IRequestHandler<CreateAnnotationCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public CreateAnnotationCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }
        
    public async Task<int> Handle(CreateAnnotationCommand request, CancellationToken cancellationToken)
    {
        var annotation = _mapper.Map<Annotation>(request);
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Employee.CorporateEmail == _currentUserService.Email, 
                cancellationToken);
      
        //TODO: Poner esto en un validator
        if (user is null)
        {
            throw new NotFoundException("User not found!");
        }

        annotation.UserId = user.Id;
        _context.Annotations.Add(annotation);
        await _context.SaveChangesAsync(cancellationToken);
        return annotation.Id;
    }
}