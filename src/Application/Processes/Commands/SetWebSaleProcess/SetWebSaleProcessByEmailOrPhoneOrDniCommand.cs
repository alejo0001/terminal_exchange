using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Processes.Commands.SetWebSaleProcess;

public class SetWebSaleProcessByEmailOrPhoneOrDniCommand: IRequest
{
    public string? Email { get; set;  }
    public string? Phone { get; set;  }
    public string? Dni { get; set;  }
}


public class SetWebSaleProcessByEmailOrPhoneOrDniCommandHandler : IRequestHandler<SetWebSaleProcessByEmailOrPhoneOrDniCommand>
    {
        
        private readonly IApplicationDbContext _context;
        private readonly IDateTime _dateTime;
        
        public SetWebSaleProcessByEmailOrPhoneOrDniCommandHandler(IApplicationDbContext context, IDateTime dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        public async Task<Unit> Handle(SetWebSaleProcessByEmailOrPhoneOrDniCommand request, CancellationToken cancellationToken)
        {

            var contact = await _context.Contact
                .Include(c => c.ContactEmail)
                .Include(c => c.ContactPhone)
                .Where(c => c.ContactEmail.Any(e => e.Email == request.Email) 
                            || c.IdCard == request.Dni
                            || c.ContactPhone.Any(e => e.Phone == request.Phone))
                .FirstOrDefaultAsync(cancellationToken);

            if (contact != null)
            {
                    var process = await _context.Processes
                        .Where(p => p.ContactId == contact.Id && !p.IsDeleted)
                        .Where(p => p.Status == ProcessStatus.ToDo ||  p.Status == ProcessStatus.Ongoing)
                        .Where(p => p.Outcome == ProcessOutcome.Open || p.Outcome == ProcessOutcome.Pending)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (process != null)
                    {
                        process.Status = ProcessStatus.Closed;
                        process.Outcome = ProcessOutcome.NotSale;

                        var newActionSaleWeb = new Action
                        {
                            UserId = process.UserId,
                            ContactId = contact.Id,
                            Date = _dateTime.Now,
                            ProcessId = process.Id,
                            Type = ActionType.WebSale,
                            Outcome = ActionOutcome.Sale
                        };
                        _context.Actions.Add(newActionSaleWeb);
                        await _context.SaveChangesAsync(cancellationToken);    
                    }
            }

            return Unit.Value;
        }
    }