using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Commands.UpdateContact;

public class UpdateContactCommandValidator : AbstractValidator<UpdateContactCommand>
{
    private readonly IApplicationDbContext _context;


    public UpdateContactCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id is required")
            .MustAsync(CheckContactExist).WithMessage("Contact not found!");

        RuleFor(v => v.ContactStatusId)
            .MustAsync(CheckContactStatus).WithMessage("Contact Status is not valid");
            
        RuleFor(v => v.ContactGenderId)
            .MustAsync(CheckContactGender).WithMessage("Contact Gender is not valid");
            
        RuleFor(v => v.ContactTypeId)
            .MustAsync(CheckContactType).WithMessage("Contact Type is not valid");
    }

    private async Task<bool> CheckContactExist(int contactId, CancellationToken cancellationToken)
    {
        return await _context.Contact.AnyAsync(c => c.Id == contactId, cancellationToken);
    }

    private async Task<bool> CheckContactStatus(UpdateContactCommand command, int? statusId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.ContactStatus.Any(c => statusId == null || c.Id == statusId), cancellationToken);
    }

    private async Task<bool> CheckContactGender(UpdateContactCommand command, int? genderId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.ContactGender.Any(c => genderId == null || c.Id == genderId), cancellationToken);
    }

    private async Task<bool> CheckContactType(UpdateContactCommand command, int? typeId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.ContactType.Any(c => typeId == null || c.Id == typeId), cancellationToken);
    }
}