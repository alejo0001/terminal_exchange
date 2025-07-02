using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CrmAPI.Application.Common.Interfaces;

public interface IAuthApiClient
{
    Task<HttpResponseMessage> AuthorizeUserInModuleWithRoles(IList<string> roles, CancellationToken ct);

    Task<HttpResponseMessage> AuthorizeUserByEmail(string email, CancellationToken ct);
}
