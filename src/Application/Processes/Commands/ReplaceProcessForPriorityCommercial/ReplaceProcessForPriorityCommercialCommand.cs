using System;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Processes.Commands.ReplaceProcessForPriorityCommercial;

[Authorize(Roles = "Usuario")]
public class ReplaceProcessForPriorityCommercialCommand : IRequest<int>
{
    public int ProcessId { get; set; }
}

[UsedImplicitly]
public class ReplaceProcessForPriorityCommercialCommandHandler
    : IRequestHandler<ReplaceProcessForPriorityCommercialCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IProcessesService _processesService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICalendarService _calendar;

    public ReplaceProcessForPriorityCommercialCommandHandler(
        IApplicationDbContext context,
        IProcessesService processesService,
        ICurrentUserService currentUserService,
        ICalendarService calendar)
    {
        _context = context;
        _calendar = calendar;
        _processesService = processesService;
        _currentUserService = currentUserService;
    }

    public async Task<int> Handle(ReplaceProcessForPriorityCommercialCommand request, CancellationToken ct)
    {
        // obtain actual user
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u =>
                u.Employee.CorporateEmail == _currentUserService.Email, ct);

        // obtain actual process
        var process = await _context.Processes
            .Include(p => p.Contact)
            .FirstOrDefaultAsync(p => p.Id == request.ProcessId, ct);

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        var processId = 0;
        try
        {
            // Close actual process
            process!.Status = ProcessStatus.Closed;
            process.Outcome = ProcessOutcome.NotSale;

            if (process.Contact.NextInteraction is null
                && process.Status is ProcessStatus.Pending or ProcessStatus.Closed
                && process.Contact.Appointments is not null
                && process.Contact.Appointments.Exists(a => !a.IsDeleted))
            {
                await _calendar.DeleteAllContactEvents(process.Contact, ct).ConfigureAwait(false);
            }

            await _processesService.ResetCoursePricesForProcess(new() { process }, ct);

            // Create new process
            // ⬇️ Si se añaden mas equipos que puedan realizar esta acción, poner visitadores por defecto no tendrá sentido
            var createProcess = new ProcessCreateDto
            {
                UserId = user!.Id,
                ContactId = process!.ContactId,
                Type = ProcessType.Visits.ToString(),
            };
            processId = await _processesService.CreateProcess(createProcess, ct);

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (Exception)
        {
            // ReSharper disable once MethodSupportsCancellation
#pragma warning disable CA2016
            await transaction.RollbackAsync();
#pragma warning restore CA2016

            throw;
        }

        return processId;
    }
}
