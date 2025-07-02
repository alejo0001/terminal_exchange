namespace CrmAPI.Contracts.Dtos;

/// <summary>Command to create missing Interested Courses for Contacts accoring using one of the possible workflows.</summary>
/// <param name="Area">
///     Area/FacultyName to work with. Accepts wildcard character <c>*</c> to work with all faculties.
/// </param>
/// <param name="CountryCode">
///     Contact country code. Accepts wildcard character <c>*</c> to work with all countries.
/// </param>
/// <param name="MaxJobContacts">
///     Must be provided in case of <em>Specific Contacts</em> workflow is intended.<br />
///     Total maximum number of contacts to work with.<br />
///     If omitted or <c>0</c>, then job will not execute.<br />
///     If wildcard value of <c>-1</c> is used then all then Contacts will be processed, that are missing
///     interested course.<br />
///     Will be ignored when <paramref name="ContactIds" /> are provided.
/// </param>
/// <param name="ContactIds">
///     Optional. If provided, then <em>Specific Contacts</em> workflow will be executed, so only these contacts are
///     analyzed for missing interested courses and <paramref name="Area" /> and <paramref name="CountryCode" />
///     will be ignored.<br />
///     Parameter <paramref name="MaxJobContacts" /> will be ignored in this case.
/// </param>
/// <remarks>
///     NB! Bear in mind, that repeated calls will have different results according to workflow:<br />
///     <em>a) Specific Contacts</em><br />
///     <em>b) Any Contacts.</em><br />
///     Repeated calls by <em>Specific Contacts</em> workflow can only have an effect on first call, if missing
///     interested courses where identified.<br />
///     Repeated calls by <em>Any Contacts</em> workflow have always potential to create new interested courses,
///     as long as missing interested courses can be identified.
/// </remarks>
public record PopulateMissingInterestedCoursesDto(
    string Area,
    string CountryCode,
    int? MaxJobContacts = 0,
    int[]? ContactIds = null)
{
    /// <remarks>Should not part of serialization.</remarks>
    public const int Unlimited = -1;

    /// <remarks>Should not part of serialization.</remarks>
    public const string Wildcard = "*";
}
