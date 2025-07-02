namespace CrmAPI.Application.Common.Interfaces;

/// <summary>
///     Inheritance is used, because DB schemas are identical.
/// </summary>
/// <remarks>
///     It is hackish, but is supposed to be temporal remedy.
/// </remarks>
public interface ICoursesFpDbContext : ICoursesUnDbContext { }
