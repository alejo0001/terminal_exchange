using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CrmAPI.Application.Common.Caching;

/// <summary>
///     .NET Memory Cache based cache. Manages key creation for each implementation type in buffered way.
/// </summary>
/// <remarks>
///     Service's behavior depends on settings dependency of <see cref="IOptionsSnapshot{TOptions}" />.
/// </remarks>
/// <typeparam name="TKey">
///     Any non-null type, because created cache keys are internally buffered and retrieval relies
///     on value equality semantics.
/// </typeparam>
/// <typeparam name="TItem">Type of items that this cache service manages internally.</typeparam>
/// <typeparam name="TOptions">
///     Type of options that must implement <see cref="ICacheSettings" />, to configure items'
///     <see cref="MemoryCacheEntryOptions" />.
/// </typeparam>
public abstract class AbstractMemoryCache<TKey, TItem, TOptions>
    : IGenericCache<TKey, TItem>
    where TKey : notnull
    where TOptions : class, ICacheSettings
{
    protected readonly ConcurrentDictionary<TKey, object> KeyBuffer = new();
    protected readonly MemoryCacheEntryOptions EntryOptions;
    protected readonly IMemoryCache MemoryCache;

    protected AbstractMemoryCache(IMemoryCache memoryCache, IOptionsSnapshot<TOptions> options)
    {
        MemoryCache = memoryCache;

        EntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(options.Value.SlidingExpirationPeriod)
            .SetAbsoluteExpiration(options.Value.AbsoluteExpirationPeriod);
    }

    /// <inheritdoc />
    public abstract object GetKey(TKey key);

    ///<inheritdoc />
    public virtual ValueTask<TItem> Set(TKey key, TItem value, CancellationToken cancellationToken) =>
        ValueTask.FromResult(MemoryCache.Set(GetBufferedKey(key), value, EntryOptions));

    /// <inheritdoc />
    public virtual ValueTask<TItem?> Get(TKey key, CancellationToken ct) =>
        ValueTask.FromResult(MemoryCache.Get<TItem>(GetBufferedKey(key)));

    protected object GetBufferedKey(TKey key) => KeyBuffer.TryGetValue(key, out var value)
        ? value
        : KeyBuffer[key] = GetKey(key);
}
