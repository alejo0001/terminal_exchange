using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace CrmAPI.Infrastructure.Services;

public class WorkingDaysService : IWorkingDays
{
    private readonly IHrApiClient _apiClient;
    private readonly IConfiguration _configuration;

    public WorkingDaysService(IConfiguration configuration, IHrApiClient apiClient)
    {
        _configuration = configuration;
        _apiClient = apiClient;
    }

    public bool IsFreeDay(Employee employee, List<Absence> absences, DateTime day )
    {
        var spainCountryId = int.Parse(_configuration["Constants:SpainCompanyCountryId"]);

        var isWeekend = day.DayOfWeek is DayOfWeek.Sunday && employee.CurrentCountryId != spainCountryId;

        var isWeekendSpanish = day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday &&
                               employee.CurrentCountryId == spainCountryId;

        if (isWeekend || isWeekendSpanish)
        {
            return true;
        }

        if (absences.Any(a => day.Date >= a.InitDate.Date && (!a.FinishDate.HasValue || day.Date <= a.FinishDate.Value.Date)))
        {
            return true;
        }

        return false;
    }

    public bool IsWeekend(Employee employee, DateTime day)
    {
        // TODO: refactorizar esto y ver donde almacenar los países q trabajan los sábados (puede q sea por nodo)
        int colombia = 136;
        return day.DayOfWeek == DayOfWeek.Sunday || day.DayOfWeek == DayOfWeek.Saturday && employee.CurrentCountryId != colombia;
    }

    public async Task<DateTime> GetNextLaboralDay(Employee employee, CancellationToken ct)
    {
        var response = await _apiClient.GetEmployeeNextLaborDay(employee.Id, ct).ConfigureAwait(false);

        var content = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        try
        {
            return DateTime.ParseExact(content, "yyyy-MM-dd", null);
        }
        catch (Exception e)
        {
            return DateTime.Now.Add(new(24, 00, 0));
        }
    }
}
