using System.Collections.Generic;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Flows.Commands.DuplicateFlow;
using CrmAPI.Application.Flows.Queries.GetFlows;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class FlowsController: ApiControllerBase
{
    /// <summary>
    /// Retrieves the list of flows.
    /// </summary>
    /// <returns>
    /// A list of <see cref="FlowDto"/> objects representing the available flows.
    /// </returns>
    /// <remarks>
    /// This method retrieves all flows based on template proposals that have a tag identifier.
    /// 
    /// Example request:
    /// 
    ///     GET /api/GetFlows
    /// </remarks>
    /// <response code="200">If the request is successfully processed and the list of flows is returned.</response>
    /// <response code="500">If an internal server error occurs.</response>        
    [HttpGet("GetFlows")]
    public async Task<ActionResult<List<FlowDto>>> GetFlows()
    {
        return await Mediator.Send(new GetFlowsQuery());
    }

    /// <summary>
    /// Duplicates an existing flow.
    /// </summary>
    /// <param name="command">
    /// Command containing the necessary parameters for the operation:
    /// - <see cref="DuplicateFlowCommand.OriginProcessType"/>: Process type of the original flow.
    /// - <see cref="DuplicateFlowCommand.OriginTagId"/>: Tag identifier of the original flow.
    /// - <see cref="DuplicateFlowCommand.TagId"/>: Tag identifier for the new flow.
    /// - <see cref="DuplicateFlowCommand.TagName"/>: Tag name for the new flow.
    /// </param>
    /// <returns>
    /// An object of type <see cref="Unit"/> indicating the operation completed successfully.
    /// </returns>
    /// <remarks>
    /// This method allows duplicating an existing flow, including its template proposals and associated templates.
    /// 
    /// Example request:
    /// 
    ///     POST /api/DuplicateFlow
    /// </remarks>
    /// <response code="200">If the request is successfully processed and the flow is duplicated.</response>
    /// <response code="400">If the request is invalid or required parameters are missing.</response>
    /// <response code="500">If an internal server error occurs.</response>        
    [HttpPost("DuplicateFlow")]
    public async Task<ActionResult<Unit>> DuplicateFlow(DuplicateFlowCommand command)
    {
        return await Mediator.Send(command);
    }
}