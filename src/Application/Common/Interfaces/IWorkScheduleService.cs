using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrmAPI.Application.Common.Interfaces;

public interface IWorkScheduleService
{
    /// <summary>
    ///     Get date for new appointment
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="dateLocalEmployee"></param>
    /// <param name="ct"></param>
    /// <returns>A date or null (async)</returns>
    public Task<DateTime?> GetProposalNextDate(int userId, DateTime dateLocalEmployee, CancellationToken ct);
}
