using System;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;

namespace CrmAPI.Application.Processes.Services;

/// <inheritdoc />
public class WorkScheduleService : IWorkScheduleService
{
    private readonly IHrApiClient _hrApiClient;

    public WorkScheduleService(IHrApiClient hrApiClient)
    {
        _hrApiClient = hrApiClient;
    }

    /// <inheritdoc />
    public async Task<DateTime?> GetProposalNextDate(int userId, DateTime dateLocalEmployee, CancellationToken ct)
    {
        if (await _hrApiClient.GetWorkScheduleDaysOfWeek(userId, dateLocalEmployee, ct) is not { } workScheduleToday)
        {
            return null;
        }

        if (!workScheduleToday.Workable)
        {
            return await TryGetProposalWithinUpcomingWeek(userId, dateLocalEmployee, ct);
        }

        var hourToCheck = dateLocalEmployee.AddHours(3).RoundDateTimeToQuarterHour();

        var isWorking = CompareSchedule(workScheduleToday, hourToCheck);
        // Translate to and return: next start of the quarter an hour.
        if (isWorking)
        {
            return hourToCheck;
        }
        if (IsInSectionGap(workScheduleToday, hourToCheck))
        {
            return CombineToNextDate(hourToCheck, workScheduleToday.SecondSectionInit.GetValueOrDefault());
        }

        return await TryGetProposalWithinUpcomingWeek(userId, dateLocalEmployee, ct);
    }

    /// <summary>
    ///     Try to calculate proposal date within next 7 days, but if it doesn't succeed returns <c>null</c>.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="dateLocalEmployee"></param>
    /// <param name="ct"></param>
    /// <returns><c>null</c>, if fails to calculate proposal date within upcoming 7 days.</returns>
    private async Task<DateTime?> TryGetProposalWithinUpcomingWeek(int userId, DateTime dateLocalEmployee, CancellationToken ct)
    {
        var dateToWorkWith = dateLocalEmployee;

        DateTime? proposalDate = null;

        for (var i = 0; i < 7; i++)
        {
            dateToWorkWith = dateToWorkWith.AddDays(1);

            var dateWorkSchedule = await _hrApiClient.GetWorkScheduleDaysOfWeek(userId, dateToWorkWith, ct);
            if (dateWorkSchedule is not { Workable: true } workSchedule)
            {
                continue;
            }

            // Check the presence of either first or second section of "dateToWorkWith".
            if (workSchedule.FirstSectionInit is { } firstInit)
            {
                return CombineToNextDate(dateToWorkWith, firstInit);
            }

            if (workSchedule.SecondSectionInit is { } secondInit)
            {
                return CombineToNextDate(dateToWorkWith, secondInit);
            }
        }

        return proposalDate;
    }

    /// <summary>
    ///     Combines in following way: parts till day from <paramref name="dateTime" />
    ///     and the rest from <paramref name="timeSpan" />.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    private static DateTime CombineToNextDate(DateTime dateTime, TimeSpan timeSpan) => new(
        dateTime.Year,
        dateTime.Month,
        dateTime.Day,
        timeSpan.Hours,
        timeSpan.Minutes,
        0);

    private static bool CompareSchedule(WorkScheduleDaysOfWeekDto workSchedule, DateTime givenDate)
    {
        // Pattern Matching is a concise way to do null check and get hold of value of base type at the same time.
        var inFirstSection = (workSchedule.FirstSectionInit, workSchedule.FirstSectionFinish) switch
        {
            ({ } start, { } end) => IsInBetweenInclusive(givenDate.TimeOfDay, start, end),
            _ => false,
        };

        if (inFirstSection)
        {
            return true;
        }

        return (workSchedule.SecondSectionInit, workSchedule.SecondSectionFinish) switch
        {
            ({ } start, { } end) => IsInBetweenInclusive(givenDate.TimeOfDay, start, end),
            _ => false,
        };
    }

    private static bool IsInSectionGap(WorkScheduleDaysOfWeekDto workSchedule, DateTime givenDate)
    {
        return givenDate.TimeOfDay > workSchedule.FirstSectionFinish &&
               givenDate.TimeOfDay < workSchedule.SecondSectionInit;
    }

    /// <summary>
    ///     Compares, if <paramref name="toCompare" /> is between <paramref name="start" /> and <paramref name="end" />.
    /// </summary>
    /// <remarks>
    ///     NB! There is no check in place that start cannot be greater than end or vice verca!
    /// </remarks>
    /// <param name="toCompare"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    private static bool IsInBetweenInclusive(TimeSpan toCompare, TimeSpan start, TimeSpan end) =>
        start <= toCompare && toCompare <= end;
}

public static class DateTimeExtension
{
    public static DateTime RoundDateTimeToQuarterHour(this DateTime dateTime)
    {
        // Redondeo a intervalos de 15 minutos
        var interval = TimeSpan.FromMinutes(15);

        // Calcular el número de intervalos de 15 minutos desde el inicio del día
        var ticksInInterval = dateTime.Ticks / interval.Ticks;

        // Calcular la posición dentro del intervalo actual
        var remainder = dateTime - new DateTime(dateTime.Ticks / interval.Ticks * interval.Ticks);

        // Definir el punto de redondeo, 7 minutos y 30 segundos
        var midpoint = TimeSpan.FromMinutes(7) + TimeSpan.FromSeconds(30);

        // Si está por debajo del punto de redondeo, redondear hacia abajo
        if (remainder < midpoint)
        {
            return new(ticksInInterval * interval.Ticks);
        }

        // Si está por encima del punto de redondeo, redondear hacia arriba
        return new((ticksInInterval + 1) * interval.Ticks);
    }
}
