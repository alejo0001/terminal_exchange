using System;
using System.Collections.Generic;
using System.Threading;
using CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses.Dtos;
using CrmAPI.Application.Settings;
using CrmAPI.Contracts.Dtos;

namespace CrmAPI.Application.Common.Interfaces;

/// <summary>
///     Analyze whether Contact is missing ContactLead.
/// </summary>
public interface IContactLeadsAnalyzerService
{
    /// <summary>
    ///     Queries projected Contacts data as <see cref="IAsyncEnumerable{T}" />, paged by value from
    ///     <see cref="InterestedCoursePopulatorSettings.ContactsQueryMaxPageSize" /> or size of
    ///     <see cref="PopulateMissingInterestedCoursesDto.ContactIds" />, whichever is smaller.
    /// </summary>
    /// <remarks>No Area nor Country Code filtering will be applied.</remarks>
    /// <param name="requestDto"></param>
    /// <param name="ct"></param>
    /// <returns>Awaitable stream.</returns>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="PopulateMissingInterestedCoursesDto.ContactIds" /> is null or empty.
    /// </exception>
    IAsyncEnumerable<List<ContactFacultyDto>> GetSpecificContactsStream(
        PopulateMissingInterestedCoursesDto requestDto,
        CancellationToken ct);

    /// <summary>
    ///     Queries projected Contacts data as <see cref="IAsyncEnumerable{T}" />, paged by value from
    ///     <see cref="InterestedCoursePopulatorSettings.ContactsQueryMaxPageSize" /> or
    ///     <see cref="PopulateMissingInterestedCoursesDto.MaxJobContacts" />, whichever is smaller.
    /// </summary>
    /// <remarks>It figures out, whether it must be short-circuited -- missing max job size.</remarks>
    /// <param name="requestDto"></param>
    /// <param name="ct"></param>
    /// <returns>Awaitable stream.</returns>
    IAsyncEnumerable<List<ContactFacultyDto>> GetAnyContactsStream(
        PopulateMissingInterestedCoursesDto requestDto,
        CancellationToken ct);
}
