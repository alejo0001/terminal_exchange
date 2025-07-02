using System.Collections.Generic;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Models;
using CrmAPI.Application.Proposals.Commands.CreateTemplate.cs;
using CrmAPI.Application.Proposals.Commands.CreateTemplateProposal;
using CrmAPI.Application.Proposals.Commands.DeleteTemplate;
using CrmAPI.Application.Proposals.Commands.DeleteTemplateInTemplateProposal;
using CrmAPI.Application.Proposals.Commands.DeleteTemplateProposal;
using CrmAPI.Application.Proposals.Commands.SetTemplateInTemplateProposal;
using CrmAPI.Application.Proposals.Commands.UpdateTemplate;
using CrmAPI.Application.Proposals.Commands.UpdateTemplateProposal;
using CrmAPI.Application.Proposals.Queries.GetAllTemplates;
using CrmAPI.Application.Proposals.Queries.GetTemplateDetails;
using CrmAPI.Application.Proposals.Queries.GetTemplateProposal;
using CrmAPI.Application.Proposals.Queries.GetTemplateProposals;
using CrmAPI.Application.Proposals.Queries.GetTemplateProposalsFromTemplate;
using CrmAPI.Application.Proposals.Queries.GetTemplatesFromProposal.GetTemplatesFromTemplateProposal;
using CrmAPI.Application.Proposals.Queries.GetTemplatesFromTagId;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class ProposalsController: ApiControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of template proposals based on the specified query parameters.
    /// </summary>
    /// <param name="query">Query parameters that include the page number, page size, and additional filters.</param>
    /// <returns>A paginated list of template proposals.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     GET /api/template-proposals?PageNumber=1&PageSize=10&QueryParams=searchString
    ///
    /// The response includes a list of available template proposals based on the specified criteria.
    /// 
    /// <response code="200">Returns a paginated list of template proposals.</response>
    /// <response code="400">If the query parameters are invalid.</response>
    /// <response code="401">If the user is not authorized to access this endpoint.</response>
    /// <response code="403">If the user does not have permission to perform this operation.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpGet("TemplateProposals")]
    public async Task<ActionResult<PaginatedList<TemplateProposalDto>>> GetTemplateProposals(
        [FromQuery] GetTemplateProposalsQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Retrieves the details of a specific template proposal.
    /// </summary>
    /// <param name="query">The query object containing the ID of the template proposal to retrieve.</param>
    /// <returns>The details of the specified template proposal.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     GET /api/template-proposal/TemplateProposal?TemplateProposalId=123
    ///
    /// Returns the details of the template proposal corresponding to the provided ID.
    /// 
    /// <response code="200">Returns the details of the specified template proposal.</response>
    /// <response code="400">If the query data is invalid.</response>
    /// <response code="404">If no template proposal is found with the specified ID.</response>
    /// <response code="401">If the user is not authorized to access this resource.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpGet("TemplateProposal")]
    public async Task<ActionResult<TemplateProposalDto>> GetTemplateProposal(
        [FromQuery] GetTemplateProposalQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Creates a new template proposal.
    /// </summary>
    /// <param name="command">The command containing the necessary data to create the template proposal.</param>
    /// <returns>The ID of the created template proposal.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     POST /api/template-proposal
    ///     {
    ///         "Name": "Template Proposal 1",
    ///         "ProcessType": "ProcessType",
    ///         "Day": 5,
    ///         "Attempt": 3,
    ///         "Colour": "#FF5733",
    ///         "CourseKnown": true,
    ///         "CourseTypeId": 10,
    ///         "HasToSendEmail": true,
    ///         "HasToSendWhatsApp": false,
    ///         "TagId": 2
    ///     }
    ///
    /// <response code="200">Returns the ID of the successfully created template proposal.</response>
    /// <response code="400">If the provided data is invalid.</response>
    /// <response code="404">If a necessary resource such as Tag or CourseType is not found.</response>
    /// <response code="401">If the user is not authorized to perform this operation.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateTemplateProposalCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Updates an existing template proposal.
    /// </summary>
    /// <param name="id">The ID of the template proposal to update.</param>
    /// <param name="command">The command containing the updated data for the template proposal.</param>
    /// <returns>An HTTP status code indicating the result of the operation.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     PUT /api/template-proposal/1
    ///     {
    ///         "Id": 1,
    ///         "Name": "Updated Template Proposal",
    ///         "ProcessType": "NewProcessType",
    ///         "Day": 10,
    ///         "Attempt": 2,
    ///         "Colour": "#3366FF",
    ///         "CourseKnown": false,
    ///         "CourseTypeId": 15,
    ///         "HasToSendEmail": false,
    ///         "HasToSendWhatsApp": true,
    ///         "TagId": 3
    ///     }
    ///
    /// <response code="204">Indicates that the template proposal was successfully updated.</response>
    /// <response code="400">If the provided ID does not match the ID in the command or if the data is invalid.</response>
    /// <response code="404">If the template proposal or any associated resource such as Tag or CourseType is not found.</response>
    /// <response code="401">If the user is not authorized to perform this operation.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateTemplateProposalCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        await Mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Deletes a template proposal by marking it as deleted.
    /// </summary>
    /// <param name="id">The ID of the template proposal to delete.</param>
    /// <returns>An HTTP status code indicating the result of the operation.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     DELETE /api/template-proposal/1
    ///
    /// Marks the specified template proposal as deleted.
    /// 
    /// <response code="204">Indicates that the template proposal was successfully deleted.</response>
    /// <response code="404">If no template proposal is found with the specified ID.</response>
    /// <response code="401">If the user is not authorized to perform this operation.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteTemplateProposalCommand { Id = id });

        return NoContent();
    }

    /// <summary>
    /// Retrieves the templates associated with a specific template proposal.
    /// </summary>
    /// <param name="query">The query parameters containing the ID of the template proposal.</param>
    /// <returns>A list of templates associated with the specified template proposal.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     GET /api/templates-from-template-proposal?TemplateProposalId=1
    ///
    /// Returns the templates associated with the template proposal that have not been deleted.
    /// 
    /// <response code="200">Returns the list of templates associated with the template proposal.</response>
    /// <response code="400">If the query parameters are invalid.</response>
    /// <response code="401">If the user is not authorized to access this endpoint.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpGet("TemplatesFromTemplateProposal")]
    public async Task<ActionResult<List<TemplateProposalTemplateDto>>> GetTemplateProposal(
        [FromQuery] GetTemplatesFromTemplateProposalQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Retrieves the templates associated with a specific tag identifier.
    /// </summary>
    /// <param name="query">The query parameters containing the ID of the tag.</param>
    /// <returns>A list of templates associated with the specified tag.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     GET /api/templates-from-tag-id?TagId=1
    ///
    /// Returns the templates that match the provided tag ID and have not been deleted.
    /// 
    /// <response code="200">Returns the list of templates associated with the tag identifier.</response>
    /// <response code="400">If the query parameters are invalid.</response>
    /// <response code="401">If the user is not authorized to access this endpoint.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpGet("TemplatesFromTagId")]
    public async Task<ActionResult<List<TemplateDto>>> GetTemplatesFromTagId(
        [FromQuery] GetTemplatesFromTagIdAndProcessTypeQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Associates a specific template with an existing template proposal.
    /// </summary>
    /// <param name="command">Command containing the identifiers of the template proposal and the template.</param>
    /// <returns>A no-content result if the operation is successful.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     PUT /api/set-template-in-template-proposal
    ///     {
    ///         "TemplateProposalId": 1,
    ///         "TemplateId": 2
    ///     }
    ///
    /// Associates the template identified by `TemplateId` with the template proposal identified by `TemplateProposalId`.
    ///
    /// <response code="204">If the template is successfully associated with the template proposal.</response>
    /// <response code="400">If the provided parameters are invalid.</response>
    /// <response code="401">If the user is not authorized to perform this operation.</response>
    /// <response code="404">If the specified template proposal or template is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpPut("SetTemplateInTemplateProposal")]
    public async Task<ActionResult> SetTemplateInTemplateProposal(SetTemplateInTemplateProposalCommand command)
    {
        await Mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Retrieves the details of a specific template by its identifier.
    /// </summary>
    /// <param name="andProcessTypeQuery">
    /// Query containing the identifier of the template.
    /// </param>
    /// <returns>An object <see cref="TemplateDetailsDto"/> with the details of the template.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     GET /TemplateDetails?TemplateId=1
    ///
    /// Retrieves the details of the template identified by `TemplateId`.
    ///
    /// <response code="200">Returns the details of the requested template.</response>
    /// <response code="400">If the provided parameters are invalid.</response>
    /// <response code="401">If the user is not authorized to perform this operation.</response>
    /// <response code="404">If the specified template is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpGet("TemplateDetails")]
    public async Task<ActionResult<TemplateDetailsDto>> GetTemplatesFromTagId(
        [FromQuery] GetTemplateDetailsQuery andProcessTypeQuery)
    {
        return await Mediator.Send(andProcessTypeQuery);
    }

    /// <summary>
    /// Retrieves a list of template proposals associated with a specific template.
    /// </summary>
    /// <param name="query">
    /// Query containing the identifier of the template.
    /// </param>
    /// <returns>A list of <see cref="TemplateProposalDto"/> objects representing the associated template proposals.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     GET /TemplatesProposalByTemplate?TemplateId=1
    ///
    /// Retrieves all template proposals associated with the template identified by `TemplateId`.
    ///
    /// <response code="200">Returns a list of template proposals associated with the specified template.</response>
    /// <response code="400">If the provided parameters are invalid.</response>
    /// <response code="401">If the user is not authorized to perform this operation.</response>
    /// <response code="404">If the specified template is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpGet("TemplatesProposalByTemplate")]
    public async Task<ActionResult<List<TemplateProposalDto>>> GetTemplateProposalsByTemplate(
        [FromQuery] GetTemplateProposalsByTemplateQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Retrieves a paginated list of all existing templates in the system.
    /// </summary>
    /// <param name="query">
    /// Object containing pagination and search parameters.
    /// </param>
    /// <returns>A paginated list of <see cref="TemplateDto"/> objects.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     GET /AllTemplates?PageNumber=1&PageSize=10&QueryParams=example
    ///
    /// Retrieves all templates with the following characteristics:
    /// - Pagination: Controlled by the `PageNumber` and `PageSize` parameters.
    /// - Search: Filtered according to specific parameters in `QueryParams`.
    ///
    /// <response code="200">Returns a paginated list of templates.</response>
    /// <response code="400">If the pagination or search parameters are invalid.</response>
    /// <response code="401">If the user is not authorized to perform this operation.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpGet("AllTemplates")]
    public async Task<ActionResult<PaginatedList<TemplateDto>>> GetAllTemplates(
        [FromQuery] GetAllTemplatesQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Creates a new template in the system.
    /// </summary>
    /// <param name="command">
    /// Command containing the necessary data to create the template.
    /// </param>
    /// <returns>The unique identifier of the newly created template.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     POST /CreateTemplate
    ///     {
    ///         "Name": "Template Name",
    ///         "Description": "Template Description",
    ///         "LanguageCode": "en"
    ///     }
    ///
    /// Creates a new template with the provided data. If a valid language code (`LanguageCode`) is specified, the template will be associated with the corresponding language.
    ///
    /// <response code="201">The template was successfully created, and its identifier is returned.</response>
    /// <response code="400">If the provided data is invalid.</response>
    /// <response code="401">If the user is not authorized to perform this operation.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpPost("CreateTemplate")]
    public async Task<ActionResult<int>> CreateTemplate(CreateTemplateCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Updates the data of an existing template.
    /// </summary>
    /// <param name="command">
    /// Command containing the updated data of the template, including its identifier.
    /// </param>
    /// <returns>A no-content result if the operation is successful.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     PUT /UpdateTemplate/1
    ///     {
    ///         "Id": 1,
    ///         "Name": "Updated Template Name",
    ///         "Label": "Updated Label",
    ///         "Subject": "Updated Subject",
    ///         "Body": "Updated Body",
    ///         "Type": "New Type",
    ///         "LanguageCode": "es",
    ///         "CourseNeeded": true,
    ///         "Order": 5,
    ///         "TagId": 10
    ///     }
    ///
    /// Updates the template identified by `Id` with the provided values. If a valid language code (`LanguageCode`) is specified, it will be associated with the corresponding language.
    ///
    /// <response code="204">If the template is successfully updated.</response>
    /// <response code="400">If the provided data is invalid.</response>
    /// <response code="401">If the user is not authorized to perform this operation.</response>
    /// <response code="404">If the specified template is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpPut("UpdateTemplate/{id}")]
    public async Task<ActionResult> UpdateTemplate(UpdateTemplateCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Deletes (marks as deleted) an existing template in the system.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the template to delete.
    /// </param>
    /// <returns>A no-content result if the operation is successful.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     DELETE /DeleteTemplate/1
    ///
    /// Marks the template identified by `id` as deleted (`IsDeleted = true`) without physically removing it from the database.
    ///
    /// <response code="204">If the template is successfully deleted.</response>
    /// <response code="400">If the provided identifier is invalid.</response>
    /// <response code="401">If the user is not authorized to perform this operation.</response>
    /// <response code="404">If the specified template is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpDelete("DeleteTemplate/{id}")]
    public async Task<ActionResult> DeleteTemplate(int id)
    {
        await Mediator.Send(new DeleteTemplateCommand { Id = id });
        return NoContent();
    }

    /// <summary>
    /// Deletes the relationships between a template and a template proposal.
    /// </summary>
    /// <param name="command">
    /// Command containing the identifiers of the template and the template proposal to unlink.
    /// </param>
    /// <returns>A no-content result if the operation is successful.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     DELETE /DeleteTemplateInTemplateProposal
    ///     {
    ///         "TemplateProposalId": 1,
    ///         "TemplateId": 2
    ///     }
    ///
    /// Marks the relationships between the template identified by `TemplateId` and the template proposal identified by `TemplateProposalId` as deleted.
    ///
    /// <response code="204">If the relationships are successfully deleted.</response>
    /// <response code="400">If the provided data is invalid.</response>
    /// <response code="401">If the user is not authorized to perform this operation.</response>
    /// <response code="404">If the relationship between the template and the template proposal is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpDelete("DeleteTemplateInTemplateProposal")]
    public async Task<ActionResult> DeleteTemplateInTemplateProposal(DeleteTemplateInTemplateProposalCommand command)
    {
        await Mediator.Send(command);

        return NoContent();
    }
}