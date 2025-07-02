using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthApiClient _apiClient;
    private readonly ICurrentUserService _currentUserService;

    public AuthService(IApplicationDbContext context, IAuthApiClient apiClient, ICurrentUserService currentUserService)
    {
        _context = context;
        _apiClient = apiClient;
        _currentUserService = currentUserService;
    }

    public async Task<bool> UserHasRole(string role)
    {
        var response = await _apiClient.AuthorizeUserInModuleWithRoles(new[] { role }, CancellationToken.None)
            .ConfigureAwait(false);

        var content = await response.Content.ReadAsStringAsync(CancellationToken.None);

        return response.StatusCode is HttpStatusCode.OK
               && bool.TryParse(content, out var isAuthorized)
               && isAuthorized;
    }

    public async Task<User?> GetCurrentUserToRequest(string? userData, string isAuthorized,
        CancellationToken cancellationToken)
    {
        // check if user is administrator
        var isAdmin = await UserHasRole(isAuthorized);
        // this var is for check if 'request.userData' is a UserId
        var isNumeric = int.TryParse(userData, out var userId);
        // initialize the query and make a copy
        var query = _context.Users.Include(u => u.Employee).AsQueryable();
        var basicQuery = query;

        if (isAdmin && userData != null) // if you are an administrator and you are performing a search of another user
        {
            if (isNumeric) // if it is a userId
            {
                query = query.Where(u => u.Id == userId);
            }
            else // if it is an employeeEmail
            {
                query = query.Where(u => u.Employee.CorporateEmail == userData);
            }

            if (!query.Any()) // if the above does not produce results
            {
                // searches the contacts of the administrator who is doing the search
                query = basicQuery.Where(u => u.Employee.CorporateEmail == _currentUserService.Email);
            }
        }
        else // if the user is not an administrator he cannot search other users' contacts and get his own contacts
        {
            query = query.Where(u => u.Employee.CorporateEmail == _currentUserService.Email);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}
