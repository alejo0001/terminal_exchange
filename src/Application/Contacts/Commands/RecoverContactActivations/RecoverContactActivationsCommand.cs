using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CrmAPI.Application.Contacts.Commands.RecoverContactActivations;

[Authorize]
public class RecoverContactActivationsCommand : IRequest<int>
{
    public int ProcessId { get; set; }
}

public class RecoverContactActivationsCommandHandler : IRequestHandler<RecoverContactActivationsCommand, int>, IRecoverContactActivationsCommand
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<RecoverContactActivationsCommandHandler> _logger;

    public RecoverContactActivationsCommandHandler(IApplicationDbContext context,
        ILogger<RecoverContactActivationsCommandHandler> logger)
    {
        _context = context;
        _logger = logger;        
    }

    public async Task<int> Handle(RecoverContactActivationsCommand request, CancellationToken ct)
    {
        var validator = new RecoverContactActivationsCommandValidator();
        var validationResult = await validator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)

        {
            _logger.LogError("Error de validación: {Errors}", validationResult.Errors);
            throw new ValidationException("Datos de entrada no válidos.", validationResult.Errors);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            if (request.ProcessId != 0)
            {
                await UpdateProcess(request.ProcessId, ct);
                await UpdateAction(request.ProcessId, ct);
            }
            else
            {
                _logger.LogError("El ID del proceso no es válido.");
            }

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return request.ProcessId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al manejar la solicitud.");
            await transaction.RollbackAsync(ct);
            throw new ApplicationException("Ocurrió un error interno en el servidor.", ex);
        }
    }

    /// <summary>
    /// Actualiza el proceso asociado al ID del usuario
    /// </summary>
    /// <param name="procesId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>    
    public async Task UpdateProcess(int processId, CancellationToken ct)
    {
        try
        {
            var newCreatedDate = DateTime.Now.AddDays(-1);

            var process = await _context.Processes
                .FirstOrDefaultAsync(p => p.Id == processId, ct);

            if (process == null)
            {
                _logger.LogError($"No se encontró un proceso con el ID {processId}.");
                return;
            }

            process.Created = newCreatedDate;            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el proceso.");
            throw;
        }
    }

    /// <summary>
    /// Actualiza las acciones asociadas a un proceso específico.
    /// </summary>
    /// <param name="procesId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task UpdateAction(int processId, CancellationToken ct)
    {
        try
        {
            var newDate = DateTime.Now.AddDays(-1);

            var actions = await _context.Actions
                .Where(a => a.ProcessId == processId)
                .ToListAsync(ct);

            if (actions == null || !actions.Any())
            {
                _logger.LogError($"No se encontraron acciones asociadas al proceso con ID {processId}.");
                return;
            }

            foreach (var action in actions)
            {
                action.Date = newDate;
                action.FinishDate = newDate;
                action.Created = newDate;
            }            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar las acciones.");
            throw;
        }
    }
}