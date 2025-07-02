using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Contracts.Dtos;

namespace CrmAPI.Application.Common.Interfaces;

public interface IManagementApiClient
{
    Task<HttpResponseMessage> GetPricesForTlmkManagementApi(
        IPricesForTlmkFromManagementApiDto parameters, CancellationToken ct);

    Task<CourseImportedTlmkDto?> GetTlmkCoursePricesManagementApiData(
        IPricesForTlmkFromManagementApiDto parameters,
        CancellationToken ct);
}