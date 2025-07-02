using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Models;
using CrmAPI.Application.Emails.Commands.SendEmailCloseProcesses;
using CrmAPI.Application.Emails.Commands.SendEmailCommand;
using CrmAPI.Application.Emails.Commands.SendEmailCommercialAssignment;
using CrmAPI.Application.Emails.Commands.SendEmailRecords2ScholarshipActivation;
using CrmAPI.Application.Emails.Queries.GetEmailDetails;
using CrmAPI.Application.Emails.Queries.GetEmailsByContactWithPagination;
using CrmAPI.Application.Emails.Queries.GetMailBoxFree;
using CrmAPI.Application.Emails.Queries.GetEmailsByContactId;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace CrmAPI.Presentation.Controllers;

public class EmailsController : ApiControllerBase
{
    /// <summary>
    /// Obtains the emails associated with a contact with pagination.
    /// </summary>
    /// <param name="query">
    /// Query that contains the necessary parameters for the operation:
    /// - <see cref="GetEmailsByContactWithPaginationQuery.ContactId"/>: Identifier of the contact.
    /// - <see cref="GetEmailsByContactWithPaginationQuery.PageNumber"/>: Page number for pagination (optional, default is 1).
    /// - <see cref="GetEmailsByContactWithPaginationQuery.PageSize"/>: Page size for pagination (optional, default is 25).
    /// - <see cref="GetEmailsByContactWithPaginationQuery.QueryParams"/>: Query parameters to filter results (optional).
    /// - <see cref="GetEmailsByContactWithPaginationQuery.OrderBy"/>: Fields by which the results are ordered (optional, default is "Id").
    /// - <see cref="GetEmailsByContactWithPaginationQuery.Order"/>: Order of the results (optional, default is "asc").
    /// </param>
    /// <returns>
    /// An object of type <see cref="PaginatedList{EmailPaginationDto}"/> representing the paginated list of found emails.
    /// </returns>
    /// <remarks>
    /// This method queries the database to retrieve the emails associated with the specified contact,
    /// applying pagination and possible filters according to the provided parameters.
    /// 
    /// Example request:
    /// 
    ///     GET /api/GetEmailsByContactWithPagination?ContactId=1&PageNumber=1&PageSize=25
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the emails are returned.</response>
    /// <response code="404">If no emails are found for the specified contact.</response>
    /// <response code="500">If an internal server error occurs.</response>    
    [HttpGet]
    public async Task<ActionResult<PaginatedList<EmailPaginationDto>>> GetEmailsByContactWithPagination(
        [FromQuery] GetEmailsByContactWithPaginationQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Sends an email with the option to attach files.
    /// </summary>
    /// <param name="command">
    /// Command that contains the necessary parameters for the operation:
    /// - <see cref="SendEmailCommand.Attachments"/>: List of attachments (optional).
    /// </param>
    /// <returns>
    /// An object of type <see cref="ActionResult{int}"/> representing the identifier of the sent email.
    /// </returns>
    /// <remarks>
    /// This method allows sending an email, including the possibility of attaching files.
    /// The operation runs asynchronously and returns the identifier of the sent email.
    /// 
    /// Example request:
    /// 
    ///     POST /api/send
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the identifier of the email is returned.</response>
    /// <response code="400">If the request is invalid or required parameters are missing.</response>
    /// <response code="500">If an internal server error occurs.</response>    
    [HttpPost]
    [DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    public async Task<ActionResult<int>> Send([FromForm] SendEmailCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Obtains the details of an email based on its identifier.
    /// </summary>
    /// <param name="id">
    /// Identifier of the email.
    /// </param>
    /// <returns>
    /// An object of type <see cref="EmailFullDto"/> representing the details of the found email.
    /// </returns>
    /// <remarks>
    /// This method queries the database to retrieve the details of the email that matches the provided identifier.
    /// 
    /// Example request:
    /// 
    ///     GET /api/emails/{id}
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the details of the email are returned.</response>
    /// <response code="404">If the email for the specified identifier is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>    
    [HttpGet("{id}")]
    public async Task<ActionResult<EmailFullDto>> GetDetails(int id)
    {
        return await Mediator.Send(new GetEmailDetailsQuery() { Id = id });
    }

    /// <summary>
    /// Checks if the mailbox is free (LOGIC MISSING).
    /// </summary>
    /// <param name="query">
    /// Query that contains the necessary parameters for the operation (currently no parameters are required).
    /// </param>
    /// <returns>
    /// An object of type <see cref="bool"/> indicating whether the mailbox is free.
    /// </returns>
    /// <remarks>
    /// This method checks the status of the current user's mailbox to determine if it is free.
    /// 
    /// Example request:
    /// 
    ///     GET /api/MailBoxFree
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the status of the mailbox is returned.</response>
    /// <response code="500">If an internal server error occurs.</response>     
    [HttpGet("MailBoxFree")]
    public async Task<ActionResult<bool>> GetMailBoxFree(
        [FromQuery] GetMailBoxFreeQuery query)
    {
        return await Mediator.Send(query);
    }
    
    /// <summary>
    /// Gets emails send to client with process and contact id.
    /// </summary>
    [HttpGet("byContactAndProcessId")]
    public async Task<ActionResult<List<EmailFullDto>>> GetEmailsByContactAndProcess([FromQuery] GetEmailByContactIdAndProcessId query)
    {
        return await Mediator.Send(query);
    }
    
    
    /// <summary>
    /// Sends an email to the client using the appropriate template.
    /// </summary>
    /// <param name="command">
    /// Object containing the parameters required to send the email:
    /// - <see cref="SendEmailCommercialAssignmentCommand.ApiKey"/>: API key required for authentication.
    /// </param>
    /// <returns>
    /// A string describing the operation's result, indicating the email delivery status.
    /// </returns>
    [Description("Send a Email to Client using correct Template.")]
    [HttpPost("SendEmailCommercialAssignment")]
    [SwaggerResponse(HttpStatusCode.OK, null)]
    public async Task<string> SendEmailRecords2(SendEmailCommercialAssignmentCommand command) => await Mediator.Send(command);

    /// <summary>
    /// Sends a scholarship activation email (R2) to the client using the appropriate template.
    /// </summary>
    /// <param name="command">
    /// Object containing the parameters required to send the email:
    /// - <see cref="SendEmailRecords2ScholarshipActivationCommand.ApiKey"/>: API key required for authentication.
    /// </param>
    /// <returns>
    /// A string describing the operation's result, indicating the email delivery status.
    /// </returns>
    [Description("Send a email of Scholarship Activation R2 to Client using correct Template.")]
    [HttpPost("SendEmailRecords2ScholarshipActivation")]
    [SwaggerResponse(HttpStatusCode.OK, null)]
    public async Task<string> SendEmailScholarshipActivation(SendEmailRecords2ScholarshipActivationCommand command) =>
        await Mediator.Send(command);

    /// <summary>
    /// Sends an email to close processes to the client using the correct template (1.2.A).
    /// </summary>
    /// <param name="command">
    /// Object containing the parameters required to send the email:
    /// - <see cref="SendEmailCloseProcessesCommand.ApiKey"/>: API key required for authentication.
    /// </param>
    /// <returns>
    /// A string describing the operation's result, indicating the email delivery status.
    /// </returns>
    [Description("Send a email for close Process to Client using correct Template (1.2.A).")]
    [HttpPost("SendEmailCloseProcesses")]
    [SwaggerResponse(HttpStatusCode.OK, null)]
    public async Task<string> SendEmailCloseProcesses(SendEmailCloseProcessesCommand command) =>
        await Mediator.Send(command);
}