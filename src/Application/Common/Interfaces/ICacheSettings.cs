using System;

namespace CrmAPI.Application.Common.Interfaces;

public interface ICacheSettings
{
    /// <summary>
    ///     For Cache Entry policy.
    /// </summary>
    TimeSpan SlidingExpirationPeriod { get; }

    /// <summary>
    ///     For Cache Entry policy.
    /// </summary>
    TimeSpan AbsoluteExpirationPeriod { get; }
}
