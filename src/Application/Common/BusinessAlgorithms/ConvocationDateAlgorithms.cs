using System;

namespace CrmAPI.Application.Common.BusinessAlgorithms;

/// <summary>
///     Something that came down from Management. Carlos, Diego and Daniel know the topic well.
/// </summary>
public static class ConvocationDateAlgorithms
{
    public static DateTime GetNextOfTheNext()
    {
        var initialNextDate = GetNext(DateTime.UtcNow);

        return GetNext(initialNextDate.AddDays(1));
    }

    public static DateTime GetNext(DateTime sinceDate)
    {
        var date = sinceDate;
        do
        {
            date = date.AddDays(1);
        } while (date.DayOfWeek is not (DayOfWeek.Monday or DayOfWeek.Wednesday or DayOfWeek.Friday));

        return date;
    }
}
