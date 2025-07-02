using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CrmAPI.Application.Common.Interfaces;

public interface ITranslationApiClient
{
    Task<HttpResponseMessage> GetForModuleAndLanguage(string languageCode, CancellationToken ct);
}
