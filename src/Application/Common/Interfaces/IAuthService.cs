using System.Threading;
using System.Threading.Tasks;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Interfaces;

public interface IAuthService
{
    public Task<bool> UserHasRole(string role);
    Task<User?> GetCurrentUserToRequest(string? userData, string isAuthorized, CancellationToken cancellationToken);
}