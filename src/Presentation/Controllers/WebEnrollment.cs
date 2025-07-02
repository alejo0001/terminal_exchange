using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.WebEnrollments.Commands.CreateWebEnrollment;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class WebEnrollmentController: ApiControllerBase
{
    /// <summary>
    /// Creates a new web enrollment record.
    /// </summary>
    /// <remarks>
    /// This endpoint allows the creation of a new web enrollment record, associating it with an existing process and contact.
    ///
    /// **Example request:**
    ///     POST /api/enrollments
    ///     Headers:
    ///         Authorization: Bearer {token}
    ///     Body:
    ///     {
    ///         "processId": 123,
    ///         "contactId": 456,
    ///         "languageCode": "EN",
    ///         ...
    ///     }    
    ///
    /// </remarks>
    /// <response code="200">Enrollment created successfully.</response>
    /// <response code="404">The specified process or contact does not exist.</response>
    /// <response code="500">Internal error while processing the request.</response>
    [HttpPost]
    public async Task<ActionResult<WebEnrollmentDto>> Create(CreateWebEnrollmentCommand command)
    {
        return await Mediator.Send(command);
    }
}


