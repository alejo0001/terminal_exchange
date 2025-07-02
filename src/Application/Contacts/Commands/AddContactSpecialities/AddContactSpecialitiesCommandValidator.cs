using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Commands.AddContactSpecialities;

public class AddContactSpecialitiesCommandValidator : AbstractValidator<AddContactSpecialitiesCommand>
{
    private readonly IApplicationDbContext _context;
        
        
    public AddContactSpecialitiesCommandValidator(IApplicationDbContext context)
    {
        _context = context;
            

        RuleFor(v => v.ContactId)
            .MustAsync(CheckContactExists).WithMessage("Contact not found!");
            
        RuleFor(v => v.SpecialitiesId)
            .MustAsync(CheckSpecialitiesExists).WithMessage("Speciality not found!");
    }
        
    private async Task<bool> CheckContactExists(AddContactSpecialitiesCommand command, int contactId, CancellationToken cancellationToken)
    {
        return await _context.Contact.AnyAsync(c => c.Id == contactId, cancellationToken);
    }
        
    private async Task<bool> CheckSpecialitiesExists(AddContactSpecialitiesCommand command, List<int> specialitiesId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.Specialities.Any(s => specialitiesId.Contains(s.Id)), cancellationToken);
    }
}