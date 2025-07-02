using System.Threading.Tasks;

namespace CrmAPI.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string UserId { get; }
    string Email { get; }
    string Name { get; }
    Task<string> JwtTokenAsync { get; }
    Task<string> GetUserOid();
    Task<string> GetUserOid(string email);
}