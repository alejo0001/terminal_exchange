namespace CrmAPI.Application.Common.Interfaces;

/// <summary>
///     REST API incoming request should
///     implement this interface to unify API security model.
/// </summary>
/// <remarks>
///     As of now REST APIs need this for sure, MassTransit should operate in secured realm already by default,
///     because those are calls between apis themselves.
/// </remarks>
public interface IHasApiKey
{
    /// <summary>
    ///     Api key.
    /// </summary>
    /// <remarks>
    ///     It can be anywhere in the HTTP request, including in header if it is comfortable for command issuers.
    /// </remarks>
    public string ApiKey { get; }
}