namespace CrmAPI.Contracts.Dtos;

/// <param name="ContactId">A Contact to which new most profitable courses will be added.</param>
/// <param name="ProcessId">A Process to which new most profitable courses will be added.</param>
/// <param name="TopCoursesCount">
///     Count of the Top Most Profitable Interested Courses to be added, in the order of
///     profitability.
/// </param>
[UsedImplicitly]
public sealed record CreateMostProfitableInterestedCoursesDto(int ContactId, int ProcessId, int TopCoursesCount);
