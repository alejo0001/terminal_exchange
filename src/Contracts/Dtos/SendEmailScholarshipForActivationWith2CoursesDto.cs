namespace CrmAPI.Contracts.Dtos;

[UsedImplicitly]
public sealed record SendEmailScholarshipForActivationWith2CoursesDto(int ContactId, int ProcessId, int[] ContactLeadIds);
