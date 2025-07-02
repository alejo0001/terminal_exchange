using System.Collections.Generic;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.ProcessSettingsTypes.Queries.GetProcessSettingsTypes;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class ProcessSettingsController : ApiControllerBase
{
    /// <summary>
    /// Retrieves a list of process configuration types.
    /// </summary>
    /// <returns>A list of objects representing the process types.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     GET /api/process-settings/types
    ///
    /// Returns a list of process configuration types that are not deleted and have an associated process type.
    /// 
    /// <response code="200">Returns the list of process configuration types.</response>
    /// <response code="401">If the user is not authorized to access this endpoint.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpGet]
    public async Task<ActionResult<List<ProcessTypeDto>>> GetProcessSettingsTypes([FromQuery] GetProcessSettingsTypeQuery query)
    {
        return await Mediator.Send(query);
    }

}