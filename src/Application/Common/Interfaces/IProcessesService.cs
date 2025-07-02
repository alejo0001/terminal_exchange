using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using Action = IntranetMigrator.Domain.Entities.Action;

namespace CrmAPI.Application.Common.Interfaces;

public interface IProcessesService
{
    Task<int> GetProcessDay(Process process);
        
    Task<int> GetProposalProcessDay(Process process);

    Task<int> ComputeAttempts(List<Action> actions, ProcessType processType);

    Task<Process> SetColourProcess(Process process, Action action, CancellationToken ct);

    Task<int> GetTotalTriesCallByDay(ProcessType processType, CancellationToken ct);
    
    Task ClearDiscountContactLeads(int contactId, CancellationToken ct);
    
    Task<bool> CheckIfProcessIsReplaceable(Process process, CancellationToken ct);
    
    Task OnCloseProcessActions(int processId, Process? process, Action? action, DiscardReasonProcess? discardReason, CancellationToken ct);
    
    Task AddDiscardReasonProcess(DiscardReasonProcess? discardReason, CancellationToken ct);
    
    Task<int> CreateProcess(ProcessCreateDto processDto, CancellationToken ct);
    
    Task<int> CreateProcess(Process process, CancellationToken ct);
    
    Task<Process> UpdateProcess(Process newDataProcess, CancellationToken ct);
    
    Task ResetCoursePricesForProcess(List<Process> processes, CancellationToken ct);
    
    Task<DateTime?> GetNextInteractionDateWhenThereIsNoResponse(Process process, DateTime dateLocalEmployee, CancellationToken cancellationToken);

    Task SetIfIsFirstInteractionDateInProcess(Action action, CancellationToken ct);

    Task<Process> UpdateProcessFromProcess(Process processNewFields, CancellationToken ct);

    Task<int> GetUserIdCorporateEmail(string corporateEmail, CancellationToken ct);
    Task<List<int>> GetContactOriginContactIdUserIdAsync(int userId, CancellationToken ct);
}