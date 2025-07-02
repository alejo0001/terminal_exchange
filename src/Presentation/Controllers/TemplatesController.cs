using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Proposals.Commands.UpdateContentInAllTemplates;
using CrmAPI.Application.Templates.Commands.SendTemplateAuditory;
using CrmAPI.Application.Templates.Commands.UpdateContentInAllTemplates;
using CrmAPI.Application.Templates.Commands.UploadTemplateAuditory;
using CrmAPI.Application.Templates.Queries.GetArgumentByProcesstId;
using CrmAPI.Application.Templates.Queries.GetTemplate;
using CrmAPI.Application.Templates.Queries.GetTemplateBundleProposal;
using CrmAPI.Application.Templates.Queries.GetTemplateByLabel;
using CrmAPI.Application.Templates.Queries.GetTemplateByNameCode;
using CrmAPI.Application.Templates.Queries.GetTemplateDetails;
using CrmAPI.Application.Templates.Queries.GetTemplateId;
using CrmAPI.Application.Templates.Queries.GetTemplates;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace CrmAPI.Presentation.Controllers;

public class TemplatesController : ApiControllerBase
{
    /// <summary>
    /// Retrieves a list of templates filtered by process type, language, and template type.
    /// </summary>
    /// <param name="query">
    /// Query containing the filters for the templates:
    /// - `ProcessType`: Type of process (e.g., "Sale", "Records").
    /// - `LanguageCode`: Language code (e.g., "en", "es").
    /// - `TemplateType`: Type of template.
    /// </param>
    /// <returns>A list of templates that match the specified filters.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     GET /api/GetTemplates?ProcessType=Sale&LanguageCode=en&TemplateType=Email
    ///
    /// Retrieves the templates filtered by process type, language, and template type. 
    /// The templates are ordered according to the following rules:
    /// 1. Templates for day 0 appear first.
    /// 2. Templates for day 99 appear last.
    /// 3. Order by colors: yellow, green, green-yellow.
    /// 4. Within the same color, they are ordered by days.
    /// 5. Within the same day, they are ordered by attempts.
    ///
    /// <response code="200">If the templates are successfully retrieved.</response>
    /// <response code="400">If the provided parameters are invalid.</response>
    /// <response code="401">If the user is not authorized to access the templates.</response>
    /// <response code="404">If no associated TagId is found for the user.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<List<TemplateDto>>> GetTemplates([FromQuery] GetTemplatesQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Retrieves the complete details of a specific template.
    /// </summary>
    /// <param name="templateId">The unique identifier of the template.</param>
    /// <returns>
    /// A `TemplateDetailsDto` object containing the details of the specified template.
    /// </returns>
    /// <remarks>
    /// Example request:
    ///
    ///     GET /api/templates/123
    ///
    /// Retrieves the details of the template with the provided ID. Includes:
    /// - Language information.
    /// - Associated template proposals (that are not deleted).
    ///
    /// <response code="200">If the template details are successfully retrieved.</response>
    /// <response code="400">If the `templateId` parameter is invalid.</response>
    /// <response code="404">If no template is found with the provided ID.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpGet("{templateId}")]
    public async Task<ActionResult<TemplateDetailsDto>> GetTemplateDetails(int templateId)
    {
        return await Mediator.Send(new GetTemplateDetailsQuery
        {
            TemplateId = templateId
        });
    }

    /// <summary>
    /// Retrieves a set of proposed templates based on a specific process and other parameters.
    /// </summary>
    /// <param name="query">An object containing the necessary filters to obtain the proposed templates.</param>
    /// <returns>
    /// A `TemplateBundleProposalViewModel` object that includes the selected templates.
    /// </returns>
    /// <remarks>
    /// Example request:
    ///
    ///     GET /api/templates/proposal?ProcessId=1&LanguageCode=es&CourseId=5&Response=true
    ///
    /// The parameters that can be sent are:
    /// - `ProcessId` (required): The unique identifier of the process.
    /// - `LanguageCode` (required): The language code (e.g., "es", "en").
    /// - `CourseId` (optional): The unique identifier of the course.
    /// - `Response` (optional): Indicates whether there was a response (true/false).
    ///
    /// <response code="200">If the set of proposed templates is generated successfully.</response>
    /// <response code="400">If any parameter is invalid.</response>
    /// <response code="404">If the associated process or related data is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpGet("Proposal")]
    public async Task<ActionResult<TemplateBundleProposalViewModel>> GetTemplateBundleProposal([FromQuery] GetTemplateBundleProposalQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Retrieves the identifier of a specific template based on various parameters.
    /// </summary>
    /// <param name="query">An object containing the necessary filters to find the template.</param>
    /// <returns>
    /// An integer representing the unique identifier of the found template.
    /// </returns>
    /// <remarks>
    /// Example request:
    ///
    ///     GET /api/templates/getId?ProcessType=Sale&TemplateType=Email&LanguageCode=es&Day=1&Attempt=1&Colour=Yellow&CourseKnown=true
    ///
    /// The parameters that can be sent are:
    /// - `ProcessType` (required): Type of process (e.g., "Sale", "Records").
    /// - `TemplateType` (required): Type of template (e.g., "Email", "WhatsApp").
    /// - `LanguageCode` (required): Language code (e.g., "es", "en").
    /// - `Day` (required): Specific day of the process flow.
    /// - `Attempt` (required): Attempt corresponding to the day.
    /// - `Colour` (required): Color associated with the process flow (e.g., "Yellow", "Green").
    /// - `CourseKnown` (required): Indicates whether the course is known (`true` or `false`).
    ///
    /// <response code="200">If the template identifier is successfully found.</response>
    /// <response code="400">If any parameter is invalid.</response>
    /// <response code="404">If no template is found that meets the criteria.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpGet("getId")]
    public async Task<ActionResult<int>> GetTemplateId([FromQuery] GetTemplateIdQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Retrieves the details of a specific template based on various parameters.
    /// </summary>
    /// <param name="query">An object containing the necessary filters to find the template.</param>
    /// <returns>
    /// A <see cref="TemplateDto"/> object containing the details of the found template.
    /// </returns>
    /// <remarks>
    /// Example request:
    ///
    ///     GET /api/templates/getTemplate?ProcessType=Sale&TemplateType=Email&LanguageCode=es&Day=1&Attempt=1&Colour=Yellow&CourseKnown=true
    ///
    /// The parameters that can be sent are:
    /// - `ProcessType` (required): Type of process (e.g., "Sale", "Records").
    /// - `TemplateType` (required): Type of template (e.g., "Email", "WhatsApp").
    /// - `LanguageCode` (required): Language code (e.g., "es", "en").
    /// - `Day` (required): Specific day of the process flow.
    /// - `Attempt` (required): Attempt corresponding to the day.
    /// - `Colour` (required): Color associated with the process flow (e.g., "Yellow", "Green").
    /// - `CourseKnown` (required): Indicates whether the course is known (`true` or `false`).
    ///
    /// <response code="200">If the template is successfully found.</response>
    /// <response code="400">If any parameter is invalid.</response>
    /// <response code="404">If no template is found that meets the criteria.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpGet("getTemplate")]
    public async Task<ActionResult<TemplateDto>> GetTemplate([FromQuery] GetTemplateQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Retrieves the details of a specific template based on its label and other parameters.
    /// </summary>
    /// <param name="query">An object containing the necessary filters to find the template.</param>
    /// <returns>
    /// A <see cref="TemplateDto"/> object containing the details of the found template.
    /// </returns>
    /// <remarks>
    /// Example request:
    ///
    ///     GET /api/templates/getTemplateByLabel?ProcessType=Sale&TemplateType=Email&LanguageCode=es&Label=Welcome
    ///
    /// The parameters that can be sent are:
    /// - `ProcessType` (required): Type of process (e.g., "Sale", "Records").
    /// - `TemplateType` (required): Type of template (e.g., "Email", "WhatsApp").
    /// - `LanguageCode` (required): Language code (e.g., "es", "en").
    /// - `Label` (required): Label that identifies the template or part of its description.
    ///
    /// <response code="200">If the template is successfully found.</response>
    /// <response code="400">If any parameter is invalid.</response>
    /// <response code="404">If no template is found that meets the criteria.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpGet("getTemplateByLabel")]
    public async Task<ActionResult<TemplateDto>> GetTemplateByLabel([FromQuery] GetTemplateByLabelQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Retrieves the details of an argument based on the process ID or the specific argument ID.
    /// </summary>
    /// <param name="query">An object containing the necessary filters to search for the argument.</param>
    /// <returns>
    /// A <see cref="TemplateDetailsDto"/> object containing the details of the found argument.
    /// </returns>
    /// <remarks>
    /// Example request:
    ///
    ///     GET /api/templates/GetArgumentByProcessId?ProcessId=123
    ///     GET /api/templates/GetArgumentByProcessId?ArgumentId=456
    ///
    /// The parameters that can be sent are:
    /// - `ProcessId` (optional): ID of the process to search for a related argument.
    /// - `ArgumentId` (optional): Specific ID of an argument.
    ///
    /// If `ArgumentId` is provided, that argument will be searched directly. 
    /// Otherwise, an attempt will be made to find an argument associated with the process specified by `ProcessId`.
    ///
    /// <response code="200">If an argument is found based on the criteria.</response>
    /// <response code="400">If no valid parameters are provided.</response>
    /// <response code="404">If no argument or associated process is found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpGet("GetArgumentByProcessId")]
    public async Task<ActionResult<TemplateDetailsDto>> GetArgumentByProcessId([FromQuery] GetArgumentByProcesstIdQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Updates a specific content in all templates that contain it.
    /// </summary>
    /// <param name="command">
    /// An object <see cref="UpdateContentInAllTemplatesCommand"/> that contains the current content to search 
    /// for and the new content to replace it.
    /// </param>
    /// <returns>
    /// An object <see cref="AffectedTemplatesViewModel"/> that includes the details of the affected templates.
    /// </returns>
    /// <remarks>
    /// Example request:
    ///
    ///     PUT /api/templates/UpdateContentInAllTemplates?ActualContent=oldValue&NewContent=newValue
    ///
    /// This endpoint updates all templates that contain the specified text in `ActualContent`
    /// by replacing it with the text provided in `NewContent`.        
    ///
    /// <response code="200">If the operation is successfully completed and the templates are updated.</response>
    /// <response code="400">If invalid or missing parameters are provided.</response>
    /// <response code="403">If the user does not have the appropriate permissions to perform this action.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpPut("UpdateContentInAllTemplates")]
    public async Task<ActionResult<AffectedTemplatesViewModel>> UpdateContentInAllTemplates([FromQuery] UpdateContentInAllTemplatesCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Uploads a file with the completed audit template. 
    /// Subsequently, it inserts the contacts that need to be audited.
    /// </summary>
    /// <param name="command">
    /// An object <see cref="UploadTemplateAuditoryCommand"/> that contains the file to upload and an optional date.
    /// </param>
    /// <returns>
    /// An HTTP response with status 204 (No Content) if the operation is successful.
    /// </returns>
    /// <remarks>
    /// Example request:
    ///
    ///     POST /api/audits/UploadTemplateAuditory
    ///     Headers:
    ///         Authorization: Bearer {token}
    ///     Body (multipart/form-data):
    ///         File: [File .xlsx/.csv/etc.]
    ///         Date: "2025-01-15" (optional)
    ///
    /// This endpoint uploads a file with audit data to blob storage. 
    /// If no date is provided, it defaults to using the next day's date.        
    ///
    /// <response code="204">If the file is uploaded successfully.</response>
    /// <response code="400">If an invalid file is provided or there are errors in the request.</response>
    /// <response code="403">If the user does not have the appropriate permissions.</response>
    /// <response code="500">If an internal error occurs during the upload process.</response>
    /// </remarks>
    [Description("Uploads a file with the completed audit template. Then, the contacts to be audited are inserted.")]
    [HttpPost("UploadTemplateAuditory")]
    public async Task<IActionResult> UploadTemplateAuditory(UploadTemplateAuditoryCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Sends an empty audit template via email.
    /// </summary>
    /// <remarks>
    /// This endpoint generates an email that includes a download link for an empty audit template. 
    /// The email is sent to the authenticated user's email address.
    ///
    /// **Requirements:**
    /// - The user must have the **Auditor** role.
    ///
    /// **Example request:**
    ///     GET /api/audits/GetTemplateAuditory
    ///     Headers:
    ///         Authorization: Bearer {token}
    ///
    /// **Expected responses:**
    /// - `204 No Content`: If the email is sent successfully.
    /// - `403 Forbidden`: If the user does not have the appropriate permissions.
    /// - `500 Internal Server Error`: If an internal error occurred.
    ///
    /// **Technical details:**
    /// - Uses a publishing service to send the email with a link to the file.
    /// - In case of an error, the event is logged for diagnostic purposes.
    /// </remarks>
    /// <response code="204">Email sent successfully.</response>
    /// <response code="403">The user does not have permission to access this resource.</response>
    /// <response code="500">Internal error while processing the request.</response>
    [Description("Send an empty template auditory by email.")]
    [HttpGet("GetTemplateAuditory")]
    [SwaggerResponse(HttpStatusCode.NoContent, null)]
    public async Task<IActionResult> GetTemplateAuditory()
    {
        await Mediator.Send(new SendTemplateAuditoryCommand());
        return NoContent();
    }

    [Description("Get a template by Name Code")]
    [HttpGet("GetTemplateByNameCode")]
    public async Task<ActionResult<TemplateDto>> GetTemplateByNameCode([FromQuery] GetTemplateByNameCodeQuery query)
    {
        return await Mediator.Send(query);
    }
}