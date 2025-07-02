using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using ErrorOr;

namespace CrmAPI.Infrastructure.ApiClients;

public class CourseFPApiClient : ICourseFPApiClient
{
    private readonly HttpClient _httpClient;
    public CourseFPApiClient(HttpClient httpClient) => _httpClient = httpClient;

    /// <inheritdoc />
    [Obsolete("Temporarily out of service, but probably will be used again in future. Should not be used!", false)]
    public async Task<ErrorOr<Created>> DoEnrollment(WebEnrollmentDto webEnrollmentDto, CancellationToken ct)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync("/api/Crm", webEnrollmentDto, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return CreateError(ex);
        }

        return response.IsSuccessStatusCode
            ? Result.Created
            : await CreateUnexpectedApiResponseError(response, ct);
    }

    private static Error CreateError(Exception ex) => Error.Failure(
        $"{nameof(CourseFPApiClient)}.{nameof(DoEnrollment)}.UnknownError",
        "Error occured while calling the API. Please try again later.",
        new() { { "Exception", ex } });

    private async Task<Error> CreateUnexpectedApiResponseError(HttpResponseMessage response, CancellationToken ct) =>
        Error.Unexpected(
            $"{nameof(CourseFPApiClient)}.{nameof(DoEnrollment)}.UnsuccessfulResult",
            "Error occured while calling the API. Please try again later.",
            new()
            {
                { nameof(response.StatusCode), response.StatusCode },
                { nameof(response.Content), await response.Content.ReadAsStringAsync(ct) },
            });
}
