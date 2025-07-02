using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CrmAPI.Application.Contacts.Commands.ConsolidateContactsByIdCommand;

/// <summary>
///     It is used to "transfer the data" from the source contact to the destination contact by ids.
/// </summary>
/// <remarks>
///     Command`s data modification is supposed to be executed in single transaction.
/// </remarks>
/// <param name="DestinationId">Contact Id where the data will be transferred.</param>
/// <param name="OriginId">Contact Id with the data to be transferred.</param>
public readonly record struct ConsolidateContactsByIdCommand(
    int DestinationId,
    int OriginId,
    string ApiKey)
    : IRequest<ErrorOr<Success>>;

[UsedImplicitly]
public class ConsolidateContactsByIdCommandHandler : IRequestHandler<ConsolidateContactsByIdCommand, ErrorOr<Success>>
{
    private readonly IContactsConsolidatorService _consolidatorService;
    private readonly ILogger<ConsolidateContactsByIdCommandHandler> _logger;

    public ConsolidateContactsByIdCommandHandler(
        IContactsConsolidatorService consolidatorService,
        ILogger<ConsolidateContactsByIdCommandHandler> logger)
    {
        _consolidatorService = consolidatorService;
        _logger = logger;
    }

    public async Task<ErrorOr<Success>> Handle(ConsolidateContactsByIdCommand request, CancellationToken ct)
    {
        var consolidateResult = await _consolidatorService.ConsolidateContacts(
            request.OriginId,
            request.DestinationId,
            ct);

        switch (consolidateResult.IsError, consolidateResult.FirstError.NumericType)
        {
            case (true, IContactsConsolidatorService.OriginContactNotFoundError):
            {
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
