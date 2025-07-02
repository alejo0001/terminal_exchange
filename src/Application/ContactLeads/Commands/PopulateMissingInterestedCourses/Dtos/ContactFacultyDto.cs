namespace CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses.Dtos;

public record ContactFacultyDto
{
    public required int ContactId { get; init; }

    public required string ContactCountryCode { get; init; } = string.Empty;

    public required int FacultyId { get; init; }

    public required string FacultyName { get; init; } = string.Empty;
}
