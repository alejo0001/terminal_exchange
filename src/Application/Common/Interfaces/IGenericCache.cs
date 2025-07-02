using System.Threading;
using System.Threading.Tasks;

namespace CrmAPI.Application.Common.Interfaces;

/// <summary>
///     Generic Cache-based service, that uses strongly typed keys. This allows to abstract away cache
///     specific key type requirements, it is up to implementors to do any conversion of key during its generation.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TItem"></typeparam>
public interface IGenericCache<in TKey, TItem>
{
    /// <summary>
    ///     Creates a key, that is suitable for underlying cache technology. Up to implementor.
    /// </summary>
    /// <param name="key">Key instance.</param>
    /// <returns></returns>
    object GetKey(TKey key);

    ValueTask<TItem> Set(TKey key, TItem value, CancellationToken ct);

    ValueTask<TItem?> Get(TKey key, CancellationToken ct);
}
