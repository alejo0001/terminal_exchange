using System.Threading.Tasks;
using CrmAPI.Application.Messages.Commands.SendMessageCommand;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class MessagesController : ApiControllerBase
{
    /// <summary>
    /// Sends a message to the specified contacts.
    /// </summary>
    /// <param name="command">
    /// Command containing the parameters required for the operation:
    /// - <see cref="SendMessageCommand.ProcessId"/>: Identifier of the associated process.
    /// - <see cref="SendMessageCommand.ContactId"/>: Identifier of the contact to whom the message is sent.
    /// - <see cref="SendMessageCommand.ContactLeadList"/>: List of lead identifiers to whom the message is sent.
    /// - <see cref="SendMessageCommand.Colour"/>: Color associated with the message.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult"/> object indicating the outcome of the operation.
    /// </returns>
    /// <remarks>
    /// This method allows sending a message to the specified contacts and logs the action in the system.
    /// 
    /// Example request:
    /// 
    ///     POST /api/Send?ProcessId=1&ContactId=2&Colour=Green&ContactLeadList=[1,2,3]
    /// </remarks>
    /// <response code="204">If the request is successfully processed and the message is sent.</response>
    /// <response code="403">If the user does not have permission to send the message.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost]
    public async Task<ActionResult> Send([FromQuery] SendMessageCommand command)
    {
        await Mediator.Send(command);

        return NoContent();
    }
}