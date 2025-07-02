
using System.Threading.Tasks;
using CrmAPI.Application.Whatsapps.Commands;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class WhatsappController : ApiControllerBase
{
    /// <summary>
    /// Sends a WhatsApp message using the provided data.
    /// </summary>
    /// <remarks>
    /// This endpoint allows sending a WhatsApp message to a specified contact. 
    /// The message data, including the content and the recipient, must be provided in the body of the request.
    ///
    /// **Example request:**
    ///     POST /api/whatsapp
    ///     Headers:
    ///         Authorization: Bearer {token}
    ///     Body:
    ///     {
    ///         "contactId": 123,
    ///         "message": "Hello, this is a test message."
    ///     }
    ///
    /// </remarks>
    /// <response code="200">Message sent successfully and recorded in the database.</response>
    /// <response code="403">The user does not have permission to perform this action.</response>
    /// <response code="500">Internal error while processing the request.</response>
    [HttpPost]
    public async Task<ActionResult<int>> Create(SendWhatsappCommand command)
    {
        return await Mediator.Send(command);
    }
}

