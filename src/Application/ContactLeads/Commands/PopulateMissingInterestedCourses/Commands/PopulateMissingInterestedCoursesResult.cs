using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses.Commands;

public class PopulateMissingInterestedCoursesResult
{
    [SetsRequiredMembers]
    public PopulateMissingInterestedCoursesResult(DateTime dateStart, DateTime dateEnd)
        => (StatisticsPeriodStart, StatisticsPeriodEnd) = (dateStart, dateEnd);

    public required DateTime StatisticsPeriodStart { get; init; }

    public required DateTime StatisticsPeriodEnd { get; init; }

    public int TotalCoursesCreated => CreatedByFacultyCountries.SelectMany(x => x.Value.Values).Sum();

    /// <summary>
    ///     Reasons can be:<br />
    ///     1. No sold courses statistics found by provided criteria,<br />
    ///     2. No matching non-deleted coursed for statistics,<br />
    ///     3. Minimum required Course graph incomplete (CourseType &amp;&amp; Faculty &amp;&amp; Country)
    /// </summary>
    public int TotalCoursesSkipped => SkippedByFacultyCounties.SelectMany(x => x.Value.Values).Sum();

    /// <summary>
    ///     Count only in case of <em>Specific Contacts</em> workflow. <c>null</c>/absent value means that this workflow
    ///     was not executed and counting is not relevant.
    /// </summary>
    public int? NotFoundContactsCount { get; private set; }

    [UsedImplicitly]
    public ConcurrentDictionary<string, ConcurrentDictionary<string, int>> CreatedByFacultyCountries { get; } = new();

    [UsedImplicitly]
    public ConcurrentDictionary<string, ConcurrentDictionary<string, int>> SkippedByFacultyCounties { get; } = new();

    [UsedImplicitly]
    public ConcurrentBag<Exception> Exceptions { get; } = new();

    /// <summary>
    ///     Update unsuccessful job step.<br />
    ///     <inheritdoc cref="TotalCoursesSkipped" />
    /// </summary>
    public void UpdateOnSuccess(string facultyName, string countryCode, int count) =>
        StoreResult(CreatedByFacultyCountries, facultyName, countryCode, count);

    public void UpdateOnSkipped(string facultyName, string countryCode, int count) =>
        StoreResult(SkippedByFacultyCounties, facultyName, countryCode, count);

    private static void StoreResult(
        ConcurrentDictionary<string, ConcurrentDictionary<string, int>> dictionary,
        string facultyName,
        string countryCode,
        int count)
    {
        if (dictionary.TryGetValue(facultyName, out var facultyCountries))
        {
            facultyCountries[countryCode] = facultyCountries.TryGetValue(countryCode, out var countryCount)
                ? countryCount + count
                : count;
        }
        else
        {
            dictionary[facultyName] = new(new[] { KeyValuePair.Create(countryCode, count) });
        }
    }

    public void SaveException(Exception exception) => Exceptions.Add(exception);

    public void SetNotFoundContactsCount(int count)
    {
        if (count > 0)
        {
            NotFoundContactsCount = count;
        }
    }
}
