using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CrmAPI.Application.Employees.Queries.GetAllManagerSubordinates;

public class GetAllManagerSubordinatesQuery : IRequest<List<EmployeeSubordinateViewModel>> { }

[UsedImplicitly]
public class GetAllManagerSubordinatesQueryHandler
    : IRequestHandler<GetAllManagerSubordinatesQuery, List<EmployeeSubordinateViewModel>>
{
    private readonly IApplicationDbContext _context;
    private readonly IHrApiClient _apiClient;
    private readonly ICurrentUserService _currentUserService;

    public GetAllManagerSubordinatesQueryHandler(
        IApplicationDbContext context,
        IHrApiClient apiClient,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _apiClient = apiClient;
        _currentUserService = currentUserService;
    }

    public async Task<List<EmployeeSubordinateViewModel>> Handle(
        GetAllManagerSubordinatesQuery request,
        CancellationToken cancellationToken)
    {
        var employeeId = await _context.Users
            .Where(u => u.Employee.CorporateEmail == _currentUserService.Email)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync(cancellationToken);

        if (employeeId is null)
        {
            // TODO: Log it at least; or consumer should do it?
            return new();
        }

        var response = await _apiClient.GetAllManagerSubordinates(employeeId.Value, cancellationToken)
            .ConfigureAwait(false);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var employeeSubordinateList = JsonConvert.DeserializeObject<List<EmployeeSubordinateViewModel>>(content);

        return employeeSubordinateList;
    }
}
