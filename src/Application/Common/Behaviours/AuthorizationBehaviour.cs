using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using MediatR;

namespace CrmAPI.Application.Common.Behaviours;

public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IAuthApiClient _apiClient;
    private readonly ICurrentUserService _currentUserService;

    public AuthorizationBehaviour(IAuthApiClient apiClient, ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
        _apiClient = apiClient;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var authorizeAttribute = request.GetType().GetCustomAttribute<AuthorizeAttribute>();

        if (authorizeAttribute == null)
        {
            return await next();
        }

        // Must be authenticated user
        // TODO: Fix ICurrentUserService.JwtTokenAsync because it is confusing and error prone.
        if (_currentUserService.UserId == null || _currentUserService.JwtTokenAsync == null)
        {
            throw new UnauthorizedAccessException();
        }

        if (string.IsNullOrEmpty(authorizeAttribute.Roles))
        {
            return await next();
        }

        var roles = authorizeAttribute.Roles.Split(";");

        var response = await _apiClient.AuthorizeUserInModuleWithRoles(roles, ct).ConfigureAwait(false);

        var content = await response.Content.ReadAsStringAsync(ct);

        if (IsNotAuthenticated(response, content))
        {
            throw new UnauthorizedAccessException();
        }

        // User is authorized / authorization not required
        return await next();
    }

    private static bool IsNotAuthenticated(HttpResponseMessage response, string content) =>
        response.StatusCode != HttpStatusCode.OK
        || !bool.TryParse(content, out var isAuthorized)
        || !isAuthorized;
}
