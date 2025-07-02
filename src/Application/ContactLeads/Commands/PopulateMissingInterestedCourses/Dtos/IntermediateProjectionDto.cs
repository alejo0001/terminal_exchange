using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses.Dtos;

internal record IntermediateProjectionDto
{
    public required int ContactId { get; init; }

    public required Faculty FirstFaculty { get; init; }

    public string ContactCountryCode { get; set; } = string.Empty;
}
