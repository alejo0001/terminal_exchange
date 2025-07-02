using CrmAPI.Contracts.Dtos;
using MassTransit;

namespace CrmAPI.Contracts.Commands;

/// <remarks>
///     TODO: Describe Profitability calculation rules!
/// </remarks>
public interface ICreateMostProfitableInterestedCourses : CorrelatedBy<NewId>
{
    public CreateMostProfitableInterestedCoursesDto Dto { get; }
}
