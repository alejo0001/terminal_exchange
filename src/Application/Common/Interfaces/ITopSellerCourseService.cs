using System;
using System.Collections.Generic;
using CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses.Dtos;
using CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses.Services;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Interfaces;

/// <summary>
///     Get Top <em>X</em> sold Courses, from TLMK and Intranet databases; where <em>X</em> will be
///     implementation detail of concrete service.
/// </summary>
public interface ITopSellerCourseService
    : IGenericCache<TopSellerCourseCacheKey, IDictionary<TopSellerCoursesStatsDto, Course>>
{
    DateTime DateStart { get; }

    DateTime DateEnd { get; }
}
