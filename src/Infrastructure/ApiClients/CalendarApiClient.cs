using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;

namespace CrmAPI.Infrastructure.ApiClients;

public class CalendarApiClient : ICalendarApiClient
{
    private const string EventsRoute = "/api/Events";

    private readonly HttpClient _httpClient;
    public CalendarApiClient(HttpClient httpClient) => _httpClient = httpClient;

    /// <inheritdoc />
    public async Task<HttpResponseMessage> CreateEvent(EventDto eventDto, CancellationToken ct) =>
        await _httpClient.PostAsJsonAsync(EventsRoute, eventDto, ct).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<HttpResponseMessage> UpdateEvent(EventDto eventDto, CancellationToken ct) =>
        await _httpClient.PutAsJsonAsync(EventsRoute, eventDto, ct).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<HttpResponseMessage> DeleteEvent(string eventId, CancellationToken ct)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new(EventsRoute, UriKind.Relative),
            Content = JsonContent.Create(new { eventId }),
        };

        return await _httpClient.SendAsync(request, ct).ConfigureAwait(false);
    }
}
