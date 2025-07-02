using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;

namespace CrmAPI.Infrastructure.ApiClients;

public class HrApiClient : IHrApiClient
{
    private const string HttpMessageDateTimeFormat = "R";

    private readonly HttpClient _httpClient;
    public HrApiClient(HttpClient httpClient) => _httpClient = httpClient;

    /// <inheritdoc />
    public async Task<HttpResponseMessage> GetAllManagerSubordinates(int employeeId, CancellationToken ct)
    {
        var uri = $"/api/Employees/SubordinatesOf/{employeeId}";

        return await _httpClient.GetAsync(uri, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResponseMessage> GetEmployeeManager(int employeeId, CancellationToken ct)
    {
        var uri = $"api/Employees/{employeeId}/Manager";

        return await _httpClient.GetAsync(uri, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<HttpResponseMessage> GetEmployeeNextLaborDay(int employeeId, CancellationToken ct)
    {
        var uri = $"api/Employees/GetNextLaborDay/{employeeId}";

        return await _httpClient.GetAsync(uri, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    /// <remarks>
    ///     This endpoint's use case in HrApi has a bug (as of 06/08/2024) that will throw,
    ///     if data cannot be found from DB. Problem is that it throws at least one unhandled exception.
    ///     That said, HTTP500 and HTTP204 or response w/o body (or even data carrying headers) should be treated the same --
    ///     no data to return for the caller.<br />
    ///     For the sake of reference: first occurence of the bug
    ///     <a
    ///         href="https://bitbucket.org/auladigital/human-resources-back/src/c083983917f662310762992ce3d0d98265e7129e/src/Application/WorkSchedules/Queries/GetWorkScheduleOfEmployeeQuery/GetWorkScheduleOfEmployeeQuery.cs#lines-42">
    ///         is here, note the date and branch
    ///     </a>
    ///     . <br />
    ///     In short: referenced method lacks required null dereference check and this is well known bug condition.
    /// </remarks>
    public async Task<WorkScheduleDaysOfWeekDto?> GetWorkScheduleDaysOfWeek(
        int userId,
        DateTime date,
        CancellationToken ct)
    {
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams.Add(new()
        {
            { "userId", userId.ToString() },
            { "date", date.ToString(HttpMessageDateTimeFormat, CultureInfo.InvariantCulture) },
        });

        var uri = new Uri($"api/WorkSchedules/GetWorkScheduleOfEmployee?{queryParams}", UriKind.Relative);

        var response = await _httpClient.GetAsync(uri, ct);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        // It might be good idea to react if authentication has failed, or validation, by analysing the response.
        // For now, we ensure that current method response will return null above and next, we try to deserialize it,
        // that can also result as null if content cannot parsed to object of specified type.
        return await response.Content.ReadFromJsonAsync<WorkScheduleDaysOfWeekDto?>(default(JsonSerializerOptions), ct);
    }
}
