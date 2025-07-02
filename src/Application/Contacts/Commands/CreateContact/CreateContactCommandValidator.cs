using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using FluentValidation;

namespace CrmAPI.Application.Contacts.Commands.CreateContact;

public class CreateContactCommandValidator : AbstractValidator<CreateContactCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateContactCommandValidator(IApplicationDbContext context)
    {
        _context = context;


        RuleFor(v => v.ContactStatusId)
            .MustAsync(CheckContactStatus).WithMessage("Contact Status is not valid");
            
        RuleFor(v => v.ContactGenderId)
            .MustAsync(CheckContactGender).WithMessage("Contact Gender is not valid");
            
        RuleFor(v => v.IdCard)
            .MustAsync(CheckContactIdCard).WithMessage("Contact IdCard already in DB");
            
        RuleFor(v => v.ContactTypeId)
            .MustAsync(CheckContactType).WithMessage("Contact Type is not valid");
            
        RuleFor(v => v.ContactEmail)
            .MustAsync(CheckContactEmail).WithMessage("Contact email already in DB");

    }
       
    private async Task<bool> CheckContactIdCard(CreateContactCommand command, string idCard,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(idCard))
            return await Task.Run(() => !_context.Contact.Any(x => x.IdCard == idCard && !x.IsDeleted), cancellationToken);
            
        return true;
    }

    private async Task<bool> CheckContactEmail(CreateContactCommand command, List<ContactEmailCreateDto> contactEmailCreateDtos, CancellationToken cancellationToken)
    {
        if (contactEmailCreateDtos == null) 
            return true;
            
        var emails = contactEmailCreateDtos.Select(e => e.Email).ToList();
            
        return await Task.Run(() => !_context.ContactEmail.Any(c => emails.Contains(c.Email) && !c.IsDeleted), cancellationToken);
    }

    private async Task<bool> CheckContactStatus(CreateContactCommand command, int statusId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.ContactStatus.Any(c => c.Id == statusId), cancellationToken);
    }
        
    private async Task<bool> CheckContactGender(CreateContactCommand command, int genderId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.ContactGender.Any(c => c.Id == genderId), cancellationToken);
    }
        
    private async Task<bool> CheckContactType(CreateContactCommand command, int typeId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.ContactType.Any(c => c.Id == typeId), cancellationToken);
    }
}