using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CrmAPI.Application.Contacts.Commands.ConsolidateContactsCommand;

/// <summary>
///     It is used to transfer the data from the source email to the destination email.
/// </summary>
/// <remarks>
///     Command`s data modification is supposed to be executed in single transaction.
/// </remarks>
/// <param name="DestinationEmail">Email where the data will be transferred.</param>
/// <param name="OriginEmail">Email with the data to be transferred.</param>
[Authorize]
public readonly record struct ConsolidateContactsCommand(string DestinationEmail, string OriginEmail)
    : IRequest<ErrorOr<Success>>;

[UsedImplicitly]
public class ConsolidateContactsCommandHandler : IRequestHandler<ConsolidateContactsCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;
    private readonly IContactsConsolidatorService _consolidatorService;
    private readonly ILogger<ConsolidateContactsCommandHandler> _logger;

    public ConsolidateContactsCommandHandler(
        IApplicationDbContext context,
        IContactsConsolidatorService consolidatorService,
        ILogger<ConsolidateContactsCommandHandler> logger)
    {
        _context = context;
        _consolidatorService = consolidatorService;
        _logger = logger;
    }

    public async Task<ErrorOr<Success>> Handle(ConsolidateContactsCommand request, CancellationToken ct)
    {
        // We rely on validation (pipeline) to check existence, to reduce validation code here!
        var destinationContactId = await _context.ContactEmail
            .Where(ce => !ce.IsDeleted && ce.Email == request.DestinationEmail)
            .Select(ce => ce.Contact.Id)
            .FirstAsync(ct);

        // We rely on validation (pipeline) to check existence, to reduce validation code here!
        var originContactId = await _context.ContactEmail
            .Where(ce => !ce.IsDeleted && ce.Email == request.OriginEmail)
            .Select(ce => ce.Contact.Id)
            .FirstAsync(ct);

        if (destinationContactId.Equals(originContactId))
        {
            var error = Error.Conflict(
                $"{nameof(ConsolidateContactsCommand)}.ConflictContactIds",
                "Both emails refer to same Id.",
                new() { { "ContactId", destinationContactId } });

            _logger.LogError("Use case halted. {@Error}", error);

            return error;
        }

        var consolidateResult = await _consolidatorService.ConsolidateContacts(
            originContactId,
            destinationContactId,
            ct);

        switch (consolidateResult.IsError, consolidateResult.FirstError.NumericType)
        {
            case (true, IContactsConsolidatorService.OriginContactNotFoundError):
            {
                consolidateResult.FirstError.Metadata?.TryAdd(
                    nameof(request.OriginEmail),
                    request.OriginEmail);

                _logger.LogError("Use case halted. {@Error}", consolidateResult.FirstError);

                return consolidateResult;
            }

            case (true, IContactsConsolidatorService.OriginLeadsNotFoundError):
            {
                _logger.LogWarning("Use case continues. {@Error}", consolidateResult.FirstError);
                break;
            }
        }

        var saveResult = await _consolidatorService.SaveChanges(ct);
        if (saveResult.IsError)
        {
            _logger.LogCritical("Use case failure. {@Error}", saveResult.FirstError);
        }

        return saveResult;
    }
}
