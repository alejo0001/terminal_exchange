using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Emails.Queries.GetEmailsByContactId;

public class GetEmailByContactIdAndProcessIdValidator : AbstractValidator<GetEmailByContactIdAndProcessId>
{
    private readonly IApplicationDbContext _context;

    public GetEmailByContactIdAndProcessIdValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.ProcessId)
            .NotEmpty().WithMessage("Process ID is required")
            .MustAsync(CheckProcessExist).WithMessage("Process ID does not exist");

        RuleFor(x => x.ContactId)
            .NotEmpty().WithMessage("Contact ID is required")
            .MustAsync(CheckContactIdExist).WithMessage("Contact ID does not exist");
        
        RuleFor(x => new { x.ProcessId, x.ContactId })
            .MustAsync((ids, cancellationToken) => CheckMatchContactIdWithProcess(ids.ProcessId, ids.ContactId, cancellationToken))
            .WithMessage("This contact ID does not match the process ID");
    }

    private Task<bool> CheckProcessExist(int processId, CancellationToken cancellationToken)
    {
        return _context.Processes.AnyAsync(p => p.Id == processId, cancellationToken);
    }

    private Task<bool> CheckContactIdExist(int contactId, CancellationToken cancellationToken)
    {
        return _context.Contact.AnyAsync(c => c.Id == contactId, cancellationToken);
    }
    
    private Task<bool> CheckMatchContactIdWithProcess(int processId, int contactId, CancellationToken cancellationToken)
    {
        return _context.Processes.AnyAsync(p => p.Id == processId && p.ContactId == contactId, cancellationToken);
    }
}