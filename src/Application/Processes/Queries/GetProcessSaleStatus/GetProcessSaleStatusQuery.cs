using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Processes.Queries.GetProcessSaleStatus;

public class GetProcessSaleStatusQuery : IRequest<ProcessSaleStatusDto>
{
    public int ProcessId { get; set; }
}
    
public class GetProcessSaleStatusQueryHandler : IRequestHandler<GetProcessSaleStatusQuery, ProcessSaleStatusDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICalendarService _calendar; 

    public GetProcessSaleStatusQueryHandler(IApplicationDbContext context, ICalendarService calendar)
    {
        _context = context;
        _calendar = calendar;
    }
        
    public async Task<ProcessSaleStatusDto> Handle(GetProcessSaleStatusQuery request, CancellationToken cancellationToken) {

        var result = false;
            
        // Buscamos el proceso y su orden asociada
        Process process = await _context.Processes
            .Where(p => p.Id == request.ProcessId)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (process == null)
            throw new NotFoundException("Process not found!");

        // Buscamos si el proceso que nos han pasado tiene una orden asociada y si 
        // esta en los estados y outcome correctos
        if (process.Status is ProcessStatus.Pending or ProcessStatus.Closed &&
            process.Outcome == ProcessOutcome.PaymentMethodPending && process.OrdersImportedId is not null)
        {
            result = true;
                
            Contact contact = await _context.Contact
                .Include(c => c.Appointments.Where(a => !a.IsDeleted))
                .Where(c => c.Id == process.ContactId)
                .FirstOrDefaultAsync(cancellationToken);

            await _calendar.DeleteAllContactEvents(contact, cancellationToken).ConfigureAwait(false);
        }

        // Enviamos la respuesta al front para que actúe en concecuencia
        ProcessSaleStatusDto processSaleStatus = new ProcessSaleStatusDto
        {
            SaleComplete = result
        };

        return processSaleStatus;
    }
}