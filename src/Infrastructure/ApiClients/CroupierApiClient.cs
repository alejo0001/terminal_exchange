using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;

namespace CrmAPI.Infrastructure.ApiClients;

public class CroupierApiClient : ICroupierApiClient
{
    private readonly HttpClient _httpClient;
    public CroupierApiClient(HttpClient httpClient) => _httpClient = httpClient;

    /// <inheritdoc />
    public async Task<HttpResponseMessage> UpdateContactFromIntranet(LeadDto lead, CancellationToken ct) =>
        await _httpClient.PutAsJsonAsync("/api/UpdateContactFromIntranet", lead, ct)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<HttpResponseMessage> UpdateContactStatusFromIntranet(
        int originalContactId,
        int status,
        CancellationToken ct)
    {
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams.Add(new()
        {
            { "originContactId", originalContactId.ToString() },
            { "contactStatusId", status.ToString() },
        });

        var uri = new Uri($"api/UpdateContactStatusFromIntranet?{queryParams}", UriKind.Relative);

        return await _httpClient.PutAsync(uri, null, ct).ConfigureAwait(false);
    }
}
