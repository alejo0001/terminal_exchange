using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Interfaces;

public interface IWorkingDays
{
    bool IsFreeDay(Employee employee, List<Absence> absences, DateTime day);

    bool IsWeekend(Employee employee, DateTime day);

    Task<DateTime> GetNextLaboralDay(Employee employee, CancellationToken ct);
}