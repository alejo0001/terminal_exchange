using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Contracts.Dtos;

namespace CrmAPI.Application.Common.Interfaces;

public interface ICourseUnApiClient
{
    [Obsolete(
        "Not in use; there is even 'v2' of this API available, if this method enters into service in the future ")]
    Task<HttpResponseMessage> GetMultiplePricesByCode(PricesByCodeCourseParamsDto parameters, CancellationToken ct);

    Task<HttpResponseMessage> GetPricesForTlmk(IPricesForTlmkParamsDto parameters, CancellationToken ct);

    [Obsolete("Temporarily out of service, but probably will be used again in future. Must not be used!", true)]
    Task<HttpResponseMessage> DoEnrollment(WebEnrollmentDto webEnrollmentDto, CancellationToken ct);
}
