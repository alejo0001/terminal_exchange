using CrmAPI.Contracts.Dtos;
using MassTransit;

namespace CrmAPI.Contracts.Commands;

[UsedImplicitly]
public record PopulateMissingInterestedCourses(PopulateMissingInterestedCoursesDto Dto) : CorrelatedBy<NewId>
{
    /// <inheritdoc />
    public NewId CorrelationId { get; init; } = NewId.Next();
}
