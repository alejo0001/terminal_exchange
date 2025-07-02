using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CrmAPI.Application.Common.Interfaces;

namespace CrmAPI.Infrastructure.ApiClients;

public class TranslationApiClient : ITranslationApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _moduleCode;

    public TranslationApiClient(HttpClient httpClient, IAppSettingsService appSettingsService)
    {
        _httpClient = httpClient;
        _moduleCode = appSettingsService["ModuleCode"];
    }

    /// <inheritdoc />
    public async Task<HttpResponseMessage> GetForModuleAndLanguage(string languageCode, CancellationToken ct)
    {
        // TODO: Migrate some of this stuff to RequestHandlerDelegate.

        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams.Add(new()
        {
            { "LanguageKey", languageCode.ToLowerInvariant() },
            { "ModuleCode", _moduleCode },
        });

        var uri = new Uri($"/api/Translations/ForModuleAndLanguage?{queryParams}", UriKind.Relative);

        return await _httpClient.GetAsync(uri, ct).ConfigureAwait(false);
    }
}
