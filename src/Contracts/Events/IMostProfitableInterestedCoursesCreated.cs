using CrmAPI.Contracts.Dtos;
using MassTransit;

namespace CrmAPI.Contracts.Events;

public interface IMostProfitableInterestedCoursesCreated : CorrelatedBy<NewId>
{
    public MostProfitableInterestedCoursesCreatedDto Dto { get; }
}
