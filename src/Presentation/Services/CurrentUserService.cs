using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Infrastructure.ApiClients.RequestHandlers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;

namespace CrmAPI.Presentation.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IAuthApiClient _authApiClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IAuthApiClient authApiClient)
    {
        _httpContextAccessor = httpContextAccessor;
        _authApiClient = authApiClient;
    }

    public string UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public string Name => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.GivenName);

    public Task<string> JwtTokenAsync => _httpContextAccessor.HttpContext?.GetTokenAsync(
        JwtBearerDefaults.AuthenticationScheme,
        AuthenticationConstants.AccessTokenName);

    public Task<string> GetUserOid() => GetUserOid(Email);

    public async Task<string> GetUserOid(string email)
    {
        var response = await _authApiClient.AuthorizeUserByEmail(email, CancellationToken.None)
            .ConfigureAwait(false);

        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }
}
