using System.Collections.Generic;
using System.Threading;
using CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses.Dtos;
using CrmAPI.Contracts.Dtos;

namespace CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses.Services;

internal delegate IAsyncEnumerable<List<ContactFacultyDto>> AnalyzerStreamBuilderDelegate(
    PopulateMissingInterestedCoursesDto requestDto,
    CancellationToken ct);
