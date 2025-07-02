namespace CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses.Dtos;

/// <summary>
///     Used to get statistics data from TLMK database.
/// </summary>
public record TopSellerCoursesStatsDto
{
    public required string FacultyName { get; init; } = string.Empty;

    public required string CourseCode { get; init; } = string.Empty;

    public required string CourseCountryCode { get; init; } = string.Empty;

    public required int SoldCount { get; init; }

    public decimal MaxPrice { get; init; }

    public decimal MaxPriceFinal { get; init; }
}
