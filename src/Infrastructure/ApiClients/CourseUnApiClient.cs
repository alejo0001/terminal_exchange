using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Contracts.Dtos;

namespace CrmAPI.Infrastructure.ApiClients;

public class CourseUnApiClient : ICourseUnApiClient
{
    private readonly HttpClient _httpClient;
    public CourseUnApiClient(HttpClient httpClient) => _httpClient = httpClient;

    /// <inheritdoc />
    public async Task<HttpResponseMessage> GetMultiplePricesByCode(
        PricesByCodeCourseParamsDto parameters,
        CancellationToken ct)
    {
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams.Add(
            new()
            {
                { nameof(parameters.CurrencyCountryCode), parameters.CurrencyCountryCode },
                { nameof(parameters.LanguageCode), parameters.LanguageCode },
                { nameof(parameters.Refresh), parameters.Refresh.ToString() },
            });

        if (!string.IsNullOrWhiteSpace(parameters.CourseCodes))
        {
            queryParams.Add(nameof(parameters.CourseCodes), parameters.CourseCodes);
        }

        if (!string.IsNullOrWhiteSpace(parameters.CountryCode))
        {
            queryParams.Add(nameof(parameters.CountryCode), parameters.CountryCode);
        }

        var uri = new Uri($"/api/courses/multiple-prices-by-code?{queryParams}", UriKind.Relative);

        return await _httpClient.GetAsync(uri, ct).ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> GetPricesForTlmk(
        IPricesForTlmkParamsDto parameters,
        CancellationToken ct)
    {
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams.Add(
            new()
            {
                { nameof(parameters.CurrencyCode), parameters.CurrencyCode },
                { nameof(parameters.LanguageCode), parameters.LanguageCode },
                { nameof(parameters.CourseTypeBaseCode), parameters.CourseTypeBaseCode },
                { nameof(parameters.AreaUrl), parameters.AreaUrl },
                { nameof(parameters.CountryId), parameters.CountryId.ToString() },
                { nameof(parameters.CourseCode), parameters.CourseCode },
                { nameof(parameters.ProcessType), parameters.ProcessType },
            });

        var uri = new Uri($"/api/courses/v2/GetPriceForTlmk?{queryParams}", UriKind.Relative);

        return await _httpClient.GetAsync(uri, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    [Obsolete("Temporarily out of service, but probably will be used again in future. Must not be used!", true)]
    public async Task<HttpResponseMessage> DoEnrollment(WebEnrollmentDto webEnrollmentDto, CancellationToken ct) =>
        await _httpClient.PostAsJsonAsync("/api/crm", webEnrollmentDto, ct).ConfigureAwait(false);
}
