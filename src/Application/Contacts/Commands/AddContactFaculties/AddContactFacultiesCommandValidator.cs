using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using FluentValidation;

namespace CrmAPI.Application.Contacts.Commands.AddContactFaculties;

public class AddContactFacultiesCommandValidator : AbstractValidator<AddContactFacultiesCommand>
{
    private readonly IApplicationDbContext _context;
        
        
    public AddContactFacultiesCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.ContactId)
            .MustAsync(CheckContactExists).WithMessage("Contact not found!");
            
        RuleFor(v => v.FacultiesId)
            .MustAsync(CheckFacultiesExists).WithMessage("Faculty not found!");
    }
        
    private async Task<bool> CheckContactExists(AddContactFacultiesCommand command, int contactId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.Contact.Any(c => c.Id == contactId), cancellationToken);
    }
        
    private async Task<bool> CheckFacultiesExists(AddContactFacultiesCommand command, List<int> facultiesId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.Faculties.Any(f => facultiesId.Contains(f.Id)), cancellationToken);
    }
}