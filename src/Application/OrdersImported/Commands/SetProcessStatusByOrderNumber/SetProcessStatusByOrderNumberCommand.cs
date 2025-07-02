using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.OrdersImported.Commands.SetProcessStatusByOrderNumber;

public class SetProcessStatusByOrderNumberCommand: IRequest<string>
{
    public int OrderNumber { get; set; }
    public ProcessStatus Status { get; set; }
    public ProcessOutcome Outcome { get; set; }
    public string PaymentType { get; set; } 
}

public class SetProcessStatusByOrderNumberCommandHandler : IRequestHandler<SetProcessStatusByOrderNumberCommand, string>
{
        
    private readonly IApplicationDbContext _context;
        
    public SetProcessStatusByOrderNumberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }


    public async Task<string> Handle(SetProcessStatusByOrderNumberCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.OrdersImported
            .Where(o => o.OrderNumber == request.OrderNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (order == null)
        {
            throw new NotFoundException("Order not found!");
        }

        var process = await _context.Processes
            .Where(p => p.Id == order.ProcessId)
            .FirstOrDefaultAsync(cancellationToken);

        process.Outcome = request.Outcome;
        process.Status = request.Status;
        order.PaymentType = request.PaymentType;

        await _context.SaveChangesAsync(cancellationToken);

        return "ok";
    }
}