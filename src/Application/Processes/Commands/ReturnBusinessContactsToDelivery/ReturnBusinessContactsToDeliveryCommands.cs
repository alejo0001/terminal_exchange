using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CrmAPI.Application.Processes.Commands.ReturnBusinessContactsToDelivery;

public record ReturnBusinessContactsToDeliveryCommands : IRequest<string>
{
    public string corporateEmail { get; set; }
    public string ApiKey { get; set; }
}

public class ReturnBusinessContactsToDeliveryHandler : IRequestHandler<ReturnBusinessContactsToDeliveryCommands, string>
{
    private readonly IProcessesService _processesService;
    private readonly ILeadsDbContext _leadsDbContext;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ReturnBusinessContactsToDeliveryHandler> _logger;

    public ReturnBusinessContactsToDeliveryHandler(IProcessesService processesService,
        ILeadsDbContext leadsDbContext,
        IApplicationDbContext context,
        ILogger<ReturnBusinessContactsToDeliveryHandler> logger)
    { 
        _processesService = processesService;
        _leadsDbContext = leadsDbContext;
        _context = context;
        _logger = logger;
    }

    public async Task<string> Handle(ReturnBusinessContactsToDeliveryCommands request, CancellationToken ct)
    {        
        var userId = await _processesService.GetUserIdCorporateEmail(request.corporateEmail, ct);
        
        var OriginContactId = await _processesService.GetContactOriginContactIdUserIdAsync(userId, ct);

        await UpdatePotencialesDatedistribution(OriginContactId, ct);

        var procesId = await GetProcesIdsByUserIdAsync(userId, ct);

        await UpdateProcesStatus(procesId, request.corporateEmail, ct);

        return "Fechas de reparto y estado de procesos actualizados correctamente.";
    }

    /// <summary>
    /// Actualizar la fecha de reparto de los contactos
    /// </summary>
    /// <param name="originContactId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task UpdatePotencialesDatedistribution(List<int> originContactId, CancellationToken ct)
    {
        if (originContactId is null or { Count: 0 })
        {
            _logger.LogError("The list of contact IDs is empty or null.");
            return;
        }

        var fechaAddMonths = DateTime.Now.AddMonths(3);

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            var leadsToUpdate = await _leadsDbContext.Leads
           .Where(l => originContactId.Contains(l.id))
           .ToListAsync(ct);

            foreach (var lead in leadsToUpdate)
            {
                lead.fecha_reparto = fechaAddMonths;

                _logger.LogInformation("Lead updated: Id = {Id}, Fecha de reparto = {FechaReparto}",
                lead.id, lead.fecha_reparto);
            }

            await _leadsDbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation("Leads updated correctly.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating the lead distribution date. IDs: {OriginContactId}", string.Join(", ", originContactId));
            throw;
        }

    }

    /// <summary>
    /// Actualizar el estado de los procesos
    /// </summary>
    /// <param name="procesIds"></param>
    /// <param name="corporateEmail"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task UpdateProcesStatus(List<int> procesIds, string corporateEmail, CancellationToken ct)
    {
        if (procesIds is null or { Count: 0 })
        {
            _logger.LogError("The list of contact IDs is empty or null.");
            return;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
            
        try
        {
            var processesToUpdate = await _context.Processes
                .Include(p => p.Contact)                 
                .Where(p => procesIds.Contains(p.Id))
                .ToListAsync(ct);

            foreach (var process in processesToUpdate)
            {
                process.Status = ProcessStatus.Closed;
                process.Outcome = ProcessOutcome.NotSale;
                process.LastModified = DateTime.UtcNow;
                process.LastModifiedBy = corporateEmail;

                _logger.LogInformation("Process updated: Id = {Id}", process.Id);
            }

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation("Processes correctly updated.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating process status. IDs: {ProcesIds}", string.Join(",", procesIds));
            throw;
        }
    }

    /// <summary>
    /// Retorna los ids de los procesos asociados a un usuario
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task<List<int>> GetProcesIdsByUserIdAsync(int userId, CancellationToken ct)
    {
        return await _context.Processes
          .AsNoTracking()
          .Include(p => p.Contact)
          .Where(p => p.UserId == userId && p.Status != ProcessStatus.Closed && (p.Type == ProcessType.Records2 || p.Type == ProcessType.Activations))
          .Where(x => !x.IsDeleted)
          .Select(p => p.Id)
          .ToListAsync(ct);
    }
}