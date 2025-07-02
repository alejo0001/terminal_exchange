using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;

namespace CrmAPI.Application.Common.Interfaces;

public interface IHrApiClient
{
    Task<HttpResponseMessage> GetAllManagerSubordinates(int employeeId, CancellationToken ct);

    Task<HttpResponseMessage> GetEmployeeManager(int employeeId, CancellationToken ct);

    Task<HttpResponseMessage> GetEmployeeNextLaborDay(int employeeId, CancellationToken ct);
    
    /// <summary>
    /// Get Work Schedule of the week
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="date"></param>
    /// <param name="ct"></param>
    /// <returns>An object of WorkScheduleDaysOfWeekDto or Null (async)</returns>
    Task<WorkScheduleDaysOfWeekDto?> GetWorkScheduleDaysOfWeek(int userId, DateTime date, CancellationToken ct);
    
    
}
