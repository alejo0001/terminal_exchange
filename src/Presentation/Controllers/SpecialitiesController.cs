using System.Collections.Generic;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Specialities.Queries.GetSpecialitiesByFaculties;
using CrmAPI.Application.Specialities.Queries.GetSpecialitiesByFaculty;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class SpecialitiesController: ApiControllerBase
{
    /// <summary>
    /// Retrieves a list of specialties associated with a specific faculty in a given country.
    /// </summary>
    /// <param name="query">
    /// Query containing the identifiers of the faculty and the country of the course.
    /// </param>
    /// <returns>A list of specialties associated with the faculty.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     GET /Faculty/List?FacultyId=1&CourseCountryId=2
    ///
    /// Retrieves the specialties associated with the faculty identified by `FacultyId`
    /// and the country identified by `CourseCountryId`.
    ///
    /// <response code="200">If the specialties are successfully retrieved.</response>
    /// <response code="400">If the provided parameters are invalid.</response>
    /// <response code="404">If the specified faculty is not found or does not have associated specialties in the specified country.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks> 
    [HttpGet("Faculty/List")]
    public async Task<List<SpecialityDto>> GetSpecialitiesByFaculty(
        [FromQuery] GetSpecialitiesByFacultyQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Retrieves a list of specialties associated with multiple faculties in a specific country.
    /// </summary>
    /// <param name="query">
    /// Query containing the identifiers of the faculties and the country of the course.
    /// </param>
    /// <returns>A list of unique specialties associated with the provided faculties.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     GET /Faculties/List?FacultiesId=1,2,3&CourseCountryId=4
    ///
    /// Retrieves the specialties associated with the faculties identified by `FacultiesId`
    /// in the country identified by `CourseCountryId`. It guarantees that the returned specialties
    /// are unique.
    ///
    /// <response code="200">If the specialties are successfully retrieved.</response>
    /// <response code="400">If the provided parameters are invalid.</response>
    /// <response code="404">If no faculties or associated specialties are found in the specified country.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>
    [HttpGet("Faculties/List")]
    public async Task<List<SpecialityDto>> GetSpecialitiesByFaculties(
        [FromQuery] GetSpecialitiesByFacultiesQuery query)
    {
        return await Mediator.Send(query);
    }
}