using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Contracts.Dtos;
using Microsoft.Extensions.Logging;

namespace CrmAPI.Infrastructure.ApiClients;

public class ManagementApiClient : IManagementApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions CourseApiJsonSerializerOptions;
    private readonly ILogger<ManagementApiClient> _logger;

    public ManagementApiClient(HttpClient httpClient, ILogger<ManagementApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<HttpResponseMessage> GetPricesForTlmkManagementApi(IPricesForTlmkFromManagementApiDto parameters, CancellationToken ct)
    {
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams.Add(
            new()
            {
                { nameof(parameters.CourseCode), parameters.CourseCode },
                { nameof(parameters.CountryId), parameters.CountryId.ToString() },
                { nameof(parameters.CourseTypeBaseCode), parameters.CourseTypeBaseCode },
                { nameof(parameters.ProcessType), parameters.ProcessType },
            });

        var uri = new Uri($"/api/courses/v2/GetPriceForTlmk?{queryParams}", UriKind.Relative);

        return await _httpClient.GetAsync(uri, ct).ConfigureAwait(false);
    }
    
    public async Task<CourseImportedTlmkDto?> GetTlmkCoursePricesManagementApiData(
        IPricesForTlmkFromManagementApiDto parameters,
        CancellationToken ct)
    {
        CourseImportedTlmkDto? courseImportedTlmk = null;
        try
        {
            var response = await GetPricesForTlmkManagementApi(parameters, ct)
                .ConfigureAwait(false);

            var responseDto = await response.Content.ReadFromJsonAsync<CourseApiResponse<CourseImportedTlmkDto>>(
                CourseApiJsonSerializerOptions,
                ct);

            courseImportedTlmk = responseDto.Data;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Problem in Course and its price data retrieval");
        }

        return courseImportedTlmk;
    }
}