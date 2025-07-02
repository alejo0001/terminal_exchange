using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Contacts.Commands.AddContactFaculties;
using FluentValidation;

namespace CrmAPI.Application.Proposals.Commands.SetTemplateInTemplateProposal;

public class SetTemplateInTemplateProposalCommandValidator: AbstractValidator<SetTemplateInTemplateProposalCommand>
{
    private readonly IApplicationDbContext _context;


    public SetTemplateInTemplateProposalCommandValidator(IApplicationDbContext context)
    {
        _context = context;
        
        RuleFor(v => v.TemplateProposalId)
            .MustAsync(CheckTemplateProposalExists).WithMessage("TemplateProposal not found!");
            
        RuleFor(v => v.TemplateId)
            .MustAsync(CheckTemplateExists).WithMessage("Template not found!");
    }
    
    private async Task<bool> CheckTemplateProposalExists(SetTemplateInTemplateProposalCommand command, int templateProposalId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.TemplateProposals.Any(c => c.Id == templateProposalId), cancellationToken);
        
    }
        
    private async Task<bool> CheckTemplateExists(SetTemplateInTemplateProposalCommand command, int templateId, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.Templates.Any(t => t.Id == templateId), cancellationToken);
    }
}