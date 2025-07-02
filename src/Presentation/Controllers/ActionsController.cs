using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Actions.Commands.CreateAction;
using CrmAPI.Application.Actions.Commands.DeleteAction;
using CrmAPI.Application.Actions.Commands.UpdateAction;
using CrmAPI.Application.Actions.Queries.GetActionCallActive;
using CrmAPI.Application.Actions.Queries.GetActionCallInfo;
using CrmAPI.Application.Actions.Queries.GetActions;
using CrmAPI.Application.Actions.Queries.GetActionsSendEmailDayZero;
using CrmAPI.Application.Actions.Queries.GetActiveCall;
using CrmAPI.Application.Common.Dtos;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace CrmAPI.Presentation.Controllers;

public class ActionsController : ApiControllerBase
{
    /// <summary>
    /// Retrieves information about a specific process, including the day, call attempts, and the maximum number of attempts.
    /// </summary>
    /// <param name="query">
    /// Parameters required to obtain action information:
    /// - <see cref="GetActionCallInfoQuery.ProcessId"/>: Unique identifier of the process.
    /// </param>
    /// <returns>
    /// An HTTP response with:
    /// - <see cref="ActionInfoDto"/>: Object containing process details such as the day, attempts, and maximum attempts.
    /// </returns>
    [HttpGet("ActionInfo")]
    public async Task<ActionInfoDto> GetActionInfo(
        [FromQuery] GetActionCallInfoQuery query) =>
        await Mediator.Send(query);

    /// <summary>
    /// Retrieves a list of actions associated with a specific process.
    /// </summary>
    /// <param name="query">
    /// Parameters required to get actions by process:
    /// - <see cref="GetActionsQuery.ProcessId"/>: Unique identifier of the process.
    /// </param>
    /// <returns>
    /// An HTTP response with:
    /// - <see cref="List{ActionChildViewModel}"/>: List of actions for the specified process.
    /// </returns>
    [HttpGet("ByProcess")]
    public async Task<ActionResult<List<ActionChildViewModel>>> GetActionsByProcess(
        [FromQuery] GetActionsQuery query) =>
        await Mediator.Send(query);

    /// <summary>
    /// Creates a new action.
    /// </summary>
    /// <param name="command">
    /// Object containing the necessary data to create an action:
    /// - <see cref="CreateActionCommand"/>: DTO with the details of the action to be created.
    /// </param>
    /// <returns>
    /// An HTTP response with:
    /// - <see cref="int"/>: Identifier of the created action.
    /// </returns>
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateActionCommand command) => await Mediator.Send(command);

    /// <summary>
    /// Updates an existing action.
    /// </summary>
    /// <param name="id">
    /// Unique identifier of the action to update.
    /// </param>
    /// <param name="command">
    /// Object containing the updated data of the action:
    /// - <see cref="UpdateActionCommand.Id"/>: Must match the `id` parameter.
    /// </param>
    /// <returns>
    /// An HTTP response with:
    /// - <see cref="NoContent"/> if the operation is successfully completed.
    /// - <see cref="BadRequest"/> if the `id` does not match the `Id` field in the command.
    /// </returns>
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateActionCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        await Mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Deletes an action.
    /// </summary>
    /// <param name="id">
    /// Unique identifier of the action to delete.
    /// </param>
    /// <returns>
    /// An HTTP response with:
    /// - <see cref="NoContent"/> if the operation is successfully completed.
    /// </returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteActionCommand { Id = id });

        return NoContent();
    }

    /// <summary>
    /// Checks if there is an active call for a specific process.
    /// </summary>
    /// <param name="getActionCallActiveQuery">
    /// Parameters required to check active calls:
    /// - <see cref="GetActionCallActiveQuery.ProcessId"/>: Unique identifier of the process.
    /// </param>
    /// <returns>
    /// An HTTP response with:
    /// - <see cref="bool"/>: `true` if there is an active call, otherwise `false`.
    /// </returns>
    [HttpGet("HasActiveCall")]
    public async Task<ActionResult<bool>> GetHasActiveCall(
        [FromQuery] GetActionCallActiveQuery getActionCallActiveQuery) =>
        await Mediator.Send(getActionCallActiveQuery);


    /// <summary>
    /// Retrieves the details of an active call for the current user.
    /// </summary>
    /// <returns>
    /// An HTTP response with:
    /// - <see cref="ActiveCallDetailsDto"/>: Object containing the details of the active call.
    /// </returns>
    [HttpGet("ActiveCall")]
    public async Task<ActionResult<ActiveCallDetailsDto>> GetActiveCall() =>
        await Mediator.Send(new GetActiveCallQuery());

    /// <summary>
    ///     Check, whether <see cref="Action" /> has it's <see cref="Action.Type" />
    ///     one of <see cref="ActionType.EmailPending" />, <see cref="ActionType.EmailSucceeded" /> or
    ///     <see cref="ActionType.EmailFailed" />.
    /// </summary>
    [HttpGet("HasSentEmails/{processId}")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(bool))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(string), Description = "No such process.")]
    public async Task<ActionResult<bool>> CheckHasSentEmails(int processId, CancellationToken ct) =>
        await Mediator.Send(new CheckHasSentEmailsQuery(processId), ct);
}
