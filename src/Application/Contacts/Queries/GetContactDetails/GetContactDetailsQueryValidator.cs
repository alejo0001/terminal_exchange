using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using FluentValidation;
using IntranetMigrator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Queries.GetContactDetails;

public class GetContactDetailsQueryValidator : AbstractValidator<GetContactDetailsQuery>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthService _authService;
    public GetContactDetailsQueryValidator(IApplicationDbContext context, 
        ICurrentUserService currentUserService, IAuthService authService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _authService = authService;

        RuleFor(v => v.ProcessId)
            .MustAsync(CheckProcessExist).WithMessage("Process does not exist");
        RuleFor(v => v.ContactId)
            .MustAsync(CheckContactExist).WithMessage("Contact does not exist");
        RuleFor(v => v)
            .MustAsync(CheckUserCanAccess).WithMessage("User can not access resource");
    }
        
    private async Task<bool> CheckProcessExist(int processId, CancellationToken cancellationToken)
    {
        return await _context.Processes.AnyAsync(p => p.Id == processId, cancellationToken);
    }
        
    private async Task<bool> CheckContactExist(int contactId, CancellationToken cancellationToken)
    {
        return await _context.Contact.AnyAsync(c => c.Id == contactId, cancellationToken);
    }
        
        
    private async Task<bool> CheckUserCanAccess(GetContactDetailsQuery query, CancellationToken cancellationToken)
    {
        if (await _authService.UserHasRole("Administrador"))
        {
            return true;
        }
            
        var user = await _context.Users
            .Include(u => u.Processes)
            .Where(u => u.Processes.Any(p => p.Id == query.ProcessId && p.ContactId == query.ContactId))
            .FirstOrDefaultAsync(u => u.Employee.CorporateEmail == _currentUserService.Email, 
                cancellationToken);

        return user?.Processes is { Count: > 0 };
    }
}