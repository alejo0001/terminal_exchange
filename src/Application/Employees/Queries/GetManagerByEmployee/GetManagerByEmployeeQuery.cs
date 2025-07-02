using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Employees.Queries.GetManagerByEmployee;

public class GetManagerByEmployeeQuery: IRequest<ManagerDto>
{
}

public class GetManagerByEmployeeQueryHandler : IRequestHandler<GetManagerByEmployeeQuery, ManagerDto>
{

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmployeeService _employeeService;

    public GetManagerByEmployeeQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IEmployeeService employeeService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _employeeService = employeeService;
    }

    public async Task<ManagerDto> Handle(GetManagerByEmployeeQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Employee.CorporateEmail == _currentUserService.Email, 
                cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User not found!");
        }

        return  await _employeeService.GetManagerByEmployee(user.Employee, cancellationToken);
    }

}