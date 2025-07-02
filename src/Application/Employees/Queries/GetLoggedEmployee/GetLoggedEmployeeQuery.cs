using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Employees.Queries.GetLoggedEmployee;

public class GetLoggedEmployeeQuery:  IRequest<EmployeeDto>
{
}

public class GetLoggedEmployeeHandler: IRequestHandler<GetLoggedEmployeeQuery, EmployeeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    private readonly IMapper _mapper;

    public GetLoggedEmployeeHandler(IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }
    
    public async Task<EmployeeDto> Handle(GetLoggedEmployeeQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Employee.CorporateEmail == _currentUserService.Email, 
                cancellationToken);

        return _mapper.Map<EmployeeDto>(user!.Employee);

    }
}