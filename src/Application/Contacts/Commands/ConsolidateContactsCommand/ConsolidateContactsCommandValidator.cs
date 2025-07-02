using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Validation;
using FluentValidation;
using IntranetMigrator.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using CascadeMode = FluentValidation.CascadeMode;

namespace CrmAPI.Application.Contacts.Commands.ConsolidateContactsCommand;

/// <remarks>
///     If you touch "existsByEmailValidator" then make sure, that <see cref="ConsolidateContactsCommandHandler" /> will
///     work, because it relies on this logic to consolidate validation here as much as possible"!
/// </remarks>
[UsedImplicitly]
public class ConsolidateContactsCommandValidator : AbstractValidator<ConsolidateContactsCommand>
{
    private readonly IApplicationDbContext _context;

    public ConsolidateContactsCommandValidator(
        IApplicationDbContext context,
        ContactExistsByEmailValidator<ConsolidateContactsCommand> existsByEmailValidator)
    {
        _context = context;

        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x)
            .Must(x => !x.OriginEmail.Trim().Equals(x.DestinationEmail.Trim(), StringComparison.OrdinalIgnoreCase))
            .WithMessage("Destination and Origin emails cannot be the same.")
            .DependentRules(() =>
            {
                RuleFor(x => x.DestinationEmail)
                    .NotEmpty()
                    .EmailAddress()
                    .SetAsyncValidator(existsByEmailValidator);

                RuleFor(x => x.OriginEmail)
                    .NotEmpty()
                    .EmailAddress()
                    .SetAsyncValidator(existsByEmailValidator)
                    .MustAsync(HaveNoActiveProcess)
                    .WithMessage("Origin contact '{PropertyValue}' can't have any active process.");
            });
    }

    private async Task<bool> HaveNoActiveProcess(string originEmail, CancellationToken ct) =>
        false == await _context.Processes
            .Where(p => p.Contact.ContactEmail
                .Where(ce => ce.Email.ToUpper().Equals(originEmail.Trim().ToUpper()))
                .Any(ce => !ce.IsDeleted))
            .Where(p => !p.Contact.IsDeleted)
            .Where(p => !p.IsDeleted)
            .Where(p => new[] { ProcessStatus.Ongoing, ProcessStatus.ToDo }.Contains(p.Status))
            .AnyAsync(ct).ConfigureAwait(false);
}
