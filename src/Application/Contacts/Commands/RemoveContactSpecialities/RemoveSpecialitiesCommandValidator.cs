using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using FluentValidation;

namespace CrmAPI.Application.Contacts.Commands.RemoveContactSpecialities;

public class RemoveContactSpecialitiesCommandValidator : AbstractValidator<RemoveContactSpecialitiesCommand>
{
    private readonly IApplicationDbContext _context;
        
    public RemoveContactSpecialitiesCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.ContactId)
            .MustAsync(CheckContactExists).WithMessage("Contact not found!");

        RuleFor(v => v.SpecialitiesId)
            .MustAsync(CheckSpecialitiesExists).WithMessage("Specialities not found!");

    }
        
    private async Task<bool> CheckContactExists(RemoveContactSpecialitiesCommand command, int contactId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.Contact.Any(c => c.Id == contactId), cancellationToken);
    }
        
    private async Task<bool> CheckSpecialitiesExists(RemoveContactSpecialitiesCommand command,
        List<int> specialitiesId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.Specialities.Any(s => specialitiesId.Contains(s.Id)), cancellationToken);
    }
}