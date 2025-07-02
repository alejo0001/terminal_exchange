using MassTransit;

namespace CrmAPI.Contracts.Commands;

[UsedImplicitly]
public record SendEmailCloseProcesses : CorrelatedBy<NewId>
{
    public NewId CorrelationId { get; init; } = NewId.Next();

    public required List<int> ProcessIds { get; init; } = new();
}