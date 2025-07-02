using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Infrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IHrApiClient _apiClient;
    public EmployeeService(IHrApiClient apiClient) => _apiClient = apiClient;

    public async Task<ManagerDto> GetManagerByEmployee(Employee employee, CancellationToken ct)
    {
        var response = await _apiClient.GetEmployeeManager(employee.Id, ct).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new();
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ManagerDto>(options: null, ct).ConfigureAwait(false)
               ?? new();
    }
}
