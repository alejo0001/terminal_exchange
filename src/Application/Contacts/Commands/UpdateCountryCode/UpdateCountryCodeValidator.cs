using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using FluentValidation;

namespace CrmAPI.Application.Contacts.Commands.UpdateCountryCode;

public class UpdateCountryCodeValidator : AbstractValidator<UpdateCountryCodeCommand>
{
        
    private readonly IApplicationDbContext _context;

    public UpdateCountryCodeValidator(IApplicationDbContext context)
    {
        _context = context;
            
        RuleFor(v => v.CountryCode)
            .NotEmpty().WithMessage("CountryCode is required")
            .MustAsync(CheckCountryCode).WithMessage("CountryCode is not valid");
            
        RuleFor(v => v.ContactId)
            .NotEmpty().WithMessage("ContactId is required")
            .MustAsync(CheckContactId).WithMessage("ContactId is not valid");
    }
        
    private async Task<bool> CheckCountryCode(string countryCode, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.Country
            .Where(c => !c.IsDeleted)
            .Any(c => c.CountryCode == countryCode.ToUpper()), cancellationToken);
    }
        
    private async Task<bool> CheckContactId(int contactId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.Contact
            .Where(c => !c.IsDeleted)
            .Any(c => c.Id == contactId), cancellationToken);
    }  
}