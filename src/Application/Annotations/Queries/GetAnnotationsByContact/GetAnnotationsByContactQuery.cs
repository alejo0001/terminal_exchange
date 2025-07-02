using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Annotations.Queries.GetAnnotationsByContact;

public class GetAnnotationsByContactQuery : IRequest<List<AnnotationDto>>
{
    public int ContactId { get; set; }
}
    
public class GetAnnotationsByContactQueryHandler : IRequestHandler<GetAnnotationsByContactQuery, List<AnnotationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;

    public GetAnnotationsByContactQueryHandler(IApplicationDbContext context, IMapper mapper,
        IAuthService authService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _authService = authService;
        _currentUserService = currentUserService;
    }
        
    public async Task<List<AnnotationDto>> Handle(GetAnnotationsByContactQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Employee.CorporateEmail == _currentUserService.Email, 
                cancellationToken);
        
        if (user is null)
        {
            throw new NotFoundException("User not found!");
        }

        var set = _context.Annotations
            .Where(c => !c.IsDeleted && c.ContactId == request.ContactId)
            .OrderByDescending(c => c.Created);

        if (!await _authService.UserHasRole("Administrador"))
        {
            set.Where(c => (c.UserId == user.Id && c.IsPrivate) || (c.UserId != user.Id && !c.IsPrivate));
        }

        return await Task.Run(() => set.AsQueryable().ProjectTo<AnnotationDto>(_mapper.ConfigurationProvider)
            .ToList(), cancellationToken);
    }
}