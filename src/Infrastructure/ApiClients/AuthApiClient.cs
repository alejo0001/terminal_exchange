using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CrmAPI.Application.Common.Interfaces;

namespace CrmAPI.Infrastructure.ApiClients;

public class AuthApiClient : IAuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _moduleCode;

    public AuthApiClient(HttpClient httpClient, IAppSettingsService appSettingsService)
    {
        _httpClient = httpClient;
        _moduleCode = appSettingsService["ModuleCode"];
    }

    /// <inheritdoc />
    public async Task<HttpResponseMessage> AuthorizeUserInModuleWithRoles(IList<string> roles, CancellationToken ct)
    {
        // TODO: Migrate some of this stuff to RequestHandlerDelegate.

        if (roles is not { Count: > 0 })
        {
            throw new ArgumentException("No roles provided for authorization op", nameof(roles));
        }

        // Gives specialized, but private derived class of NameValueCollection, that has special overload of ToString().
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams.Add("ModuleCode", _moduleCode);

        foreach (var role in roles)
        {
            queryParams.Add("Roles", role);
        }

        var uri = new Uri($"/api/Modules/AuthorizeUserInModuleWithRoles?{queryParams}", UriKind.Relative);

        return await _httpClient.GetAsync(uri, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResponseMessage> AuthorizeUserByEmail(string email, CancellationToken ct)
    {
        var uri = $"/api/Users/ByEmail/{HttpUtility.UrlEncode(email)}";

        return await _httpClient.GetAsync(uri, ct).ConfigureAwait(false);
    }
}
