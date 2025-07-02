namespace CrmAPI.Contracts.Dtos;

[UsedImplicitly]
public record MostProfitableInterestedCoursesCreatedDto(int ContactId, int ProcessId, int[] CreatedContactLeadIds);
