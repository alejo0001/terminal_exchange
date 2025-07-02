using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Security;
using CrmAPI.Application.UtcTimeZone.Services;
using MediatR;

namespace CrmAPI.Application.UtcTimeZone.Queries;

[Authorize]
public record GetUtcTimeZoneQuery(string DestinationCountryIso, DateTime DatetimeFrom) : IRequest<string>
{

}

public class GetUtcTimeZoneQueryhandler : IRequestHandler<GetUtcTimeZoneQuery, string>
{
    private readonly UtcTimeZoneService _timeZoneService;

    public GetUtcTimeZoneQueryhandler(UtcTimeZoneService timeZoneService)
    {
        _timeZoneService = timeZoneService;
    }

    public async Task<string> Handle(GetUtcTimeZoneQuery request, CancellationToken ct)
    {
        GetUtcTimeZoneQueryValidation.ValidateCountryIso(request.DestinationCountryIso);

        var convertedTime = _timeZoneService.ConvertUtcToCountryTimeAsync(request.DatetimeFrom, request.DestinationCountryIso, ct);
        return convertedTime.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
