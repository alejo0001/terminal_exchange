using System;

namespace CrmAPI.Application.Settings;

public class InterestedCoursePopulatorSettings
{
    public const string SectionName = "InterestedCoursePopulatorFeature:PopulatorSettings";

    /// <summary>
    ///     Maximum size of contacts page, that will be retrieved from DB. Careful, it is quite costly query in terms
    ///     of object graph upon filtering is applied, it's advisable not to se it too big.
    /// </summary>
    /// <remarks>
    ///     Its value is input to "clamping" as an upper bound.
    /// </remarks>
    public int ContactsQueryMaxPageSize { get; set; }

    /// <summary>
    ///     This is effectively a number of entities, that will be saved to DB with one implicit transaction.
    /// </summary>
    /// <remarks>
    ///     Contacts are retrieved by <see cref="ContactsQueryMaxPageSize" />, and this page is chunked so that each
    ///     transaction will not grow too big.
    /// </remarks>
    public int EntityCreationMaxChunkSize { get; set; }

    /// <summary></summary>
    /// <remarks>Comparison will be case-sensitive and exact-match!</remarks>
    public string[] ExcludedFaculties { get; set; } = Array.Empty<string>();
}
