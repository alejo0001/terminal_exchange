using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;

namespace CrmAPI.Infrastructure.ApiClients.RequestHandlers;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public AuthHeaderHandler(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var token = await GetToken();

        var authenticationHeaderValue = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);

        request.Headers.Authorization = authenticationHeaderValue;

        return await base.SendAsync(request, ct).ConfigureAwait(false);
    }

    private async Task<string?> GetToken()
    {
        if (_httpContextAccessor.HttpContext is not { } context)
        {
            return null;
        }

        var token = await context.GetTokenAsync(
            JwtBearerDefaults.AuthenticationScheme,
            AuthenticationConstants.AccessTokenName);

        return token;
    }
}
