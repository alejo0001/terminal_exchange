using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;

namespace CrmAPI.Application.Common.Interfaces;

public interface ICalendarApiClient
{
    Task<HttpResponseMessage> CreateEvent(EventDto eventDto, CancellationToken ct);

    Task<HttpResponseMessage> UpdateEvent(EventDto eventDto, CancellationToken ct);

    Task<HttpResponseMessage> DeleteEvent(string eventId, CancellationToken ct);
}
