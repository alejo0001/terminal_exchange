using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IntranetMigrator.Domain.Entities;
using NotificationAPI.Contracts.Commands;

namespace CrmAPI.Application.Common.Interfaces;

public interface ICloseProcessService
{
    Task<string> EmailCloseProcessStepByStep(List<int> processIdList, CancellationToken ct);
    
    Task<string> EmailCloseProcessStepByStep(List<Process> processes, CancellationToken ct);

    Task<Dictionary<Process, CreateEmail>> CreateEmailCommandsForCloseProcess(
        List<Process> processes,
        CancellationToken ct);
}