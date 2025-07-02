using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Processes.Commands.CloseProcessByOrderEmailOrPhone;

public class CloseProcessSaleByOrderNumberOrEmailCommand: List<OrderImportedUpdateDto>, IRequest
{
}
    
public class CloseProcessSalesByOrderNumberOrEmailCommandHandler : IRequestHandler<CloseProcessSaleByOrderNumberOrEmailCommand>
{
    private readonly IApplicationDbContext _context;

    public CloseProcessSalesByOrderNumberOrEmailCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(CloseProcessSaleByOrderNumberOrEmailCommand request, CancellationToken cancellationToken)
    {
        // Recogemos todos los emails y números de pedidos en dos listas
        var processOrderImportedList = new List<int?>();
        foreach (var order in request)
        {
            processOrderImportedList.Add(order.OrderNumber);
        }

        processOrderImportedList.RemoveAll(item => item == null);
        
        // Buscamos los procesos que coincidan con las listas anteriores
        var processList = await _context.Processes
            .Include(p => p.OrdersImported)
            .Where(p => processOrderImportedList.Contains(p.OrdersImported.OrderNumber))
            .Where(p => p.Status == ProcessStatus.Pending)
            .ToListAsync(cancellationToken);


        // Por cada proceso encontrado, seteamos sus datos correspondientes
        foreach (var process in processList)
        {
            var order =  request.FirstOrDefault(o => o.OrderNumber == process.OrdersImported.OrderNumber);
            
            if (order is not null)
            {
                process.OrdersImported.PaymentType = order.PaymentType;
                process.Status = ProcessStatus.Closed;
                if (order.Status == "CANCELADO")
                {
                    process.Outcome = ProcessOutcome.NotSale;
                }
                else
                {
                    process.OrdersImported.FirstPaymentDate = order.PaymentDate;
                    process.Outcome = ProcessOutcome.Sale;
                }
            }
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}