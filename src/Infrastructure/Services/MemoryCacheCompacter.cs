using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CrmAPI.Infrastructure.Services;

/// <summary>Simple MemoryCache compacter background service.</summary>
/// <remarks>
///     It relies on <see cref="IOptions{TOptions}" /> of <see cref="MemoryCacheOptions" />, configure it as needed.<br />
///     Also, (singelton )<see cref="MemoryCache" /> concrete implementation is required, because it has Compact method.
/// </remarks>
[UsedImplicitly]
public class MemoryCacheCompacter : IHostedService, IDisposable, IAsyncDisposable
{
    private Timer? _timer;

    private readonly MemoryCache _cache;
    private readonly ILogger<MemoryCacheCompacter> _logger;
    private readonly MemoryCacheOptions _settings;

    /// <exception cref="InvalidOperationException">If <see cref="MemoryCache" /> is not registered in DI.</exception>
    public MemoryCacheCompacter(
        IMemoryCache memoryCache,
        IOptions<MemoryCacheOptions> options,
        ILogger<MemoryCacheCompacter> logger)
    {
        _cache = memoryCache as MemoryCache
                 ?? throw new InvalidOperationException(
                     "MemoryCache concrete implementation is required, " +
                     "because it has Compact method on which this service relies on.");

        _settings = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken ct)
    {
        _logger.LogDebug("Cache compacter background service started");

        _timer = new(RunCompact, null, _settings.ExpirationScanFrequency, _settings.ExpirationScanFrequency);

        return Task.CompletedTask;
    }

    private void RunCompact(object? _)
    {
        var beforeStatistics = _cache.GetCurrentStatistics();

        _cache.Compact(_settings.CompactionPercentage);

        LogStatistics(beforeStatistics);

        _logger.LogInformation("Finished MemoryCache compaction");
    }

    private void LogStatistics(MemoryCacheStatistics? beforeStatistics)
    {
        if (beforeStatistics is null)
        {
            return;
        }

        var afterStatistics = _cache.GetCurrentStatistics()!;

        _logger.LogDebug(
            "Cache statistics => before: count {BeforeCount}, size {BeforeSize}; " +
            "after: count {AfterCount}, size {AfterSize}; total hits {TotalHits}, misses {TotalMisses}",
            beforeStatistics.CurrentEntryCount,
            beforeStatistics.CurrentEstimatedSize,
            afterStatistics.CurrentEntryCount,
            afterStatistics.CurrentEstimatedSize,
            beforeStatistics.TotalHits,
            beforeStatistics.TotalMisses
        );
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken ct)
    {
        Dispose();

        _logger.LogDebug("Cache compacter background service stopped");

        return Task.CompletedTask;
    }

    private void ReleaseUnmanagedResources() => _timer?.Dispose();

    /// <inheritdoc />
    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    ~MemoryCacheCompacter() => ReleaseUnmanagedResources();
}
