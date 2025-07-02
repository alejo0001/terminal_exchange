using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Interfaces;

public interface IEmployeeService
{
    Task<ManagerDto> GetManagerByEmployee(Employee employee, CancellationToken ct);
}