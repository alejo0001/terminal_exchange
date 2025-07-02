using System;
using CrmAPI.Application.Common.Interfaces;

namespace CrmAPI.Application.Settings;

public class TopSellerCourseSettings : ICacheSettings
{
    public const string SectionName = "InterestedCoursePopulatorFeature:TopSellerCourseSettings";

    /// <summary>
    ///     In case of Start period is not provided by <see cref="StatisticsPeriodStart" /> or
    ///     <see cref="StatisticsPeriod" />, then during Top Sellers statistics calculation,
    ///     this value is used to calculate start date.
    /// </summary>
    public const int DefaultMonthsBack = 12;

    /// <summary>
    ///     Exclude Courses with these payment types form Top Sellers statistic calculation.
    /// </summary>
    /// <remarks>Comparison will be case-sensitive and exact-match!</remarks>
    public string[] ExcludedPaymentTypes { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     Exclude Courses with payment type, that match following pattern form Top Sellers statistic calculation.
    /// </summary>
    /// <remarks>Input for SQL <c>LIKE</c> input and currently TLMK DB is MySQL.</remarks>
    public string ExcludePaymentTypePattern { get; set; } = string.Empty;

    /// <summary>
    ///     Max number of Courses to included, when Top Seller Course statistics are being calculated from TLMK DB.
    /// </summary>
    public int StatisticsTakeLimit { get; set; }

    /// <inheritdoc />
    public TimeSpan SlidingExpirationPeriod { get; set; }

    /// <inheritdoc />
    public TimeSpan AbsoluteExpirationPeriod { get; set; }

    /// <summary>
    ///     If present, has the second priority to period start calculation on Top Sellers statistics calculation.
    /// </summary>
    /// <remarks>
    ///     Days, hours, minutes -- <see cref="TimeSpan" /> specs :-).<br />
    ///     <inheritdoc cref="StatisticsPeriodStart" />
    /// </remarks>
    public TimeSpan? StatisticsPeriod { get; set; }

    /// <summary>
    ///     If present, has the highest property to period start calculation on Top Sellers statistics calculation.
    /// </summary>
    /// <remarks>As the lowest priority, <see cref="DefaultMonthsBack" /> will be used.</remarks>
    public DateTime? StatisticsPeriodStart { get; set; }

    /// <summary>
    ///     If not specified, "today" will be used.
    /// </summary>
    public DateTime? StatisticsPeriodEnd { get; set; }
}
