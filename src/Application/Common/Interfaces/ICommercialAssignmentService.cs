using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Interfaces;

public interface ICommercialAssignmentService
{
    /// <summary>
    ///     This data is supposed to come from HrApi, but security don`t allow to do it. So this 1:1 copy from HrApi.
    /// </summary>
    /// <param name="employee"></param>
    ManagerDto? GetEmployeeManager(Employee employee);

    string? GetReceiver(Process process);

    Task<ContactLead?> GetContactLead(int contactId, CancellationToken ct);

    /// <summary>
    ///     This is required to be called before any service's internal use of <see cref="OrganizationNode" /> data!
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task FillOrganizationNodeCache(CancellationToken ct);
}
