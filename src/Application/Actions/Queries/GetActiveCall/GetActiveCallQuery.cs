using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Actions.Queries.GetActiveCall;

public class GetActiveCallQuery : IRequest<ActiveCallDetailsDto> {
}

public class GetActiveCallByUserIdQueryHandler : IRequestHandler<GetActiveCallQuery, ActiveCallDetailsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetActiveCallByUserIdQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<ActiveCallDetailsDto> Handle(GetActiveCallQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Employee.CorporateEmail == _currentUserService.Email, 
                cancellationToken);
            
        return await _context.Actions
            .Include(a => a.Contact)
            .ThenInclude(a => a.ContactPhone.Where(p => p.IsDefault == true))
            .Include(a => a.Contact)
            .ThenInclude(a => a.ContactEmail.Where(e => e.IsDefault == true))
            .Include(a => a.Process)
            .Where(a=>a.UserId == user.Id && a.FinishDate == null && a.Type == ActionType.Call)
            .AsSplitQuery()
            .ProjectTo<ActiveCallDetailsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}