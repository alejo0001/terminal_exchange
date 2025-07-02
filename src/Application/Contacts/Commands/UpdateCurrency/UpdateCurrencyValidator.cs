using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using FluentValidation;

namespace CrmAPI.Application.Contacts.Commands.UpdateCurrency;

public class UpdateCurrencyValidator : AbstractValidator<UpdateCurrencyCommand>
{
        
    private readonly IApplicationDbContext _context;

    public UpdateCurrencyValidator(IApplicationDbContext context)
    {
        _context = context;
            
        RuleFor(v => v.CurrencyId)
            .NotEmpty().WithMessage("CurrencyId is required")
            .MustAsync(CheckCurrencyId).WithMessage("CurrencyId is not valid");
            
        RuleFor(v => v.ContactId)
            .NotEmpty().WithMessage("ContactId is required")
            .MustAsync(CheckContactId).WithMessage("ContactId is not valid");
    }
        
    private async Task<bool> CheckCurrencyId(int currencyId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.Currency
            .Where(c => !c.IsDeleted)
            .Any(c => c.Id == currencyId), cancellationToken);
    }
        
    private async Task<bool> CheckContactId(int contactId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.Contact
            .Where(c => !c.IsDeleted)
            .Any(c => c.Id == contactId), cancellationToken);
    }  
}