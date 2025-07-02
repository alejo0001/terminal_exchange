using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using FluentValidation;
using IntranetMigrator.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CrmAPI.Application.Contacts.Commands.ConsolidateContactsByIdCommand;

/// <remarks>
///     If you touch "existsByEmailValidator" then make sure, that <see cref="ConsolidateContactsByIdCommandHandler" />
///     will work, because it relies on this logic to consolidate validation here as much as possible"!
/// </remarks>
[UsedImplicitly]
public sealed class ConsolidateContactsByIdCommandValidator : AbstractValidator<ConsolidateContactsByIdCommand>
{
    private readonly IApplicationDbContext _context;

    public ConsolidateContactsByIdCommandValidator(IApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;

        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Continue;

        var apiKey = configuration["Constants:ApiKey"];

        RuleFor(r => r.ApiKey)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Equal(apiKey)
            .WithMessage("APIKEY not valid");

        RuleFor(x => x.OriginId)
            .NotEmpty();

        RuleFor(x => x.DestinationId)
            .NotEmpty();

        RuleFor(x => x.OriginId)
            .Cascade(CascadeMode.Stop)
            .NotEqual(x => x.DestinationId)
            .WithMessage("Destination and Origin IDs cannot be the same.");

        RuleFor(x => x.DestinationId)
            .MustAsync(Exists);

        RuleFor(x => x.OriginId)
            .MustAsync(Exists)
            .MustAsync(HaveNoActiveProcess)
            .WithMessage("Origin contact with ID '{PropertyValue}' can't have any active process.");
        ;
    }

    private async Task<bool> Exists(int contactId, CancellationToken ct) =>
        await _context.Contact
            .Where(c => c.Id == contactId)
            .Where(c => !c.IsDeleted)
            .AnyAsync(ct);

    private async Task<bool> HaveNoActiveProcess(int contactId, CancellationToken ct) =>
        false == await _context.Processes
            .Where(p => p.ContactId == contactId)
            .Where(p => !p.IsDeleted)
            .Where(p => new[] { ProcessStatus.Ongoing, ProcessStatus.ToDo }.Contains(p.Status))
            .AnyAsync(ct).ConfigureAwait(false);
}
