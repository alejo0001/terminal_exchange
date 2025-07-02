using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Processes.Commands.CloseProcesses;

public class CloseProcessesCommand: IRequest
{
    public List<int> ProcessIds { get; set; }
    public bool? IsAutomatic { get; set; } = false;
}
    
public class CloseProcessesCommandHandler : IRequestHandler<CloseProcessesCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IProcessesService _processesService;
    private readonly ICalendarService _calendar;

    public CloseProcessesCommandHandler(
        IApplicationDbContext context,
        IProcessesService processesService,
        ICalendarService calendar)
    {
        _context = context;
        _processesService = processesService;
        _calendar = calendar;
    }

    public async Task<Unit> Handle(CloseProcessesCommand request, CancellationToken cancellationToken)
    {
        // COMPROBAR LISTA Y FILTRAR POR PROCESOS A CERRAR
        var processes = await _context.Processes
            .Include(p => p.Contact)
                .ThenInclude(c => c.ContactLeads)
            .Include(p => p.Contact)
                .ThenInclude(c => c.ContactLanguages)
            .Include(p => p.Contact)
                .ThenInclude(c => c.ContactEmail)
            .Include(p => p.Contact)
                .ThenInclude(c => c.Gender)
            .Include(p => p.Contact)
                .ThenInclude(c => c.Appointments)
            .Where(p => request.ProcessIds.Contains(p.Id) && p.Status != ProcessStatus.Closed)
            .ToListAsync(cancellationToken);

        foreach (var process in processes)
        {
           // CERRAR PROCESO 
           process.Status = ProcessStatus.Closed;
           process.Outcome = ProcessOutcome.NotSale;
           
           if (process.Contact.NextInteraction is null  
               && process.Status is ProcessStatus.Pending or ProcessStatus.Closed
               && process.Contact.Appointments is not null 
               && process.Contact.Appointments.Any(a => !a.IsDeleted))
           {
               await _calendar.DeleteAllContactEvents(process.Contact, cancellationToken).ConfigureAwait(false);
           }
        }
        
        // ENVIAR EMAIL DESPEDIDA
       // await _processesService.SendThanksEmailCloseProcess(processes, cancellationToken);
        
        // PONER TODOS LOS DESCUENTOS DE LOS LEADS DEL CONTACTO A 0
        await _processesService.ResetCoursePricesForProcess(processes, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return default;
    }
}