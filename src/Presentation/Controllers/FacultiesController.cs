using System.Collections.Generic;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Faculties.Queries.GetFaculties;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class FacultiesController : ApiControllerBase
{
    /// <summary>
    /// Retrieves the list of faculties.
    /// </summary>
    /// <returns>
    /// A list of <see cref="FacultyDto"/> objects representing the available faculties.
    /// </returns>
    /// <remarks>
    /// This method retrieves all faculties that are not deleted.
    /// 
    /// Example request:
    /// 
    ///     GET /api/List
    /// </remarks>
    /// <response code="200">If the request is successfully processed and the list of faculties is returned.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("List")]
    public async Task<List<FacultyDto>> GetFaculties()
    {
        return await Mediator.Send(new GetFacultiesQuery());
    }
}