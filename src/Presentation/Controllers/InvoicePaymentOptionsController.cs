using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Models;
using CrmAPI.Application.InvoicePaymentOptions.Commands.CreateInvoicePaymentOption;
using CrmAPI.Application.InvoicePaymentOptions.Commands.DeleteInvoicePaymentOption;
using CrmAPI.Application.InvoicePaymentOptions.Commands.UpdateInvoicePaymentOption;
using CrmAPI.Application.InvoicePaymentOptions.Queries.GetInvoicePaymentOptionsByContactWithPagination;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class InvoicePaymentOptionsController : ApiControllerBase
{
    /// <summary>
    /// Retrieves the invoice payment options associated with a contact.
    /// </summary>
    /// <param name="query">
    /// Query containing the necessary parameters for the operation:
    /// - <see cref="GetInvoicePaymentOptionsByContactWithPaginationQuery.ContactId"/>: Contact identifier.
    /// - <see cref="GetInvoicePaymentOptionsByContactWithPaginationQuery.PageNumber"/>: Page number for pagination (optional, defaults to 1).
    /// - <see cref="GetInvoicePaymentOptionsByContactWithPaginationQuery.PageSize"/>: Page size for pagination (optional, defaults to 25).
    /// - <see cref="GetInvoicePaymentOptionsByContactWithPaginationQuery.QueryParams"/>: Query parameters to filter results (optional).
    /// - <see cref="GetInvoicePaymentOptionsByContactWithPaginationQuery.OrderBy"/>: Fields to sort results by (optional, defaults to "Id").
    /// - <see cref="GetInvoicePaymentOptionsByContactWithPaginationQuery.Order"/>: Order of results (optional, defaults to "asc").
    /// </param>
    /// <returns>
    /// A <see cref="PaginatedList{InvoicePaymentOptionDto}"/> representing the paginated list of invoice payment options found.
    /// </returns>
    /// <remarks>
    /// This method retrieves invoice payment options associated with the specified contact, applying possible filters based on the provided parameters.
    /// 
    /// Example request:
    /// 
    ///     GET /api/GetInvoicePaymentOptionsByContactWithPagination?ContactId=1&PageNumber=1&PageSize=25
    /// </remarks>
    /// <response code="200">If the request is successfully processed and the invoice payment options are returned.</response>
    /// <response code="404">If no invoice payment options are found for the specified contact.</response>
    /// <response code="500">If an internal server error occurs.</response>           
    [HttpGet]
    public async Task<ActionResult<PaginatedList<InvoicePaymentOptionDto>>> GetInvoicePaymentOptionsByContactWithPagination(
        [FromQuery] GetInvoicePaymentOptionsByContactWithPaginationQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Creates a new invoice payment option.
    /// </summary>
    /// <param name="command">
    /// Command containing the necessary parameters for the operation:
    /// - <see cref="CreateInvoicePaymentOptionCommand.Property1"/>: Description of property 1 (adjust according to InvoicePaymentOptionCreateDto properties).
    /// - <see cref="CreateInvoicePaymentOptionCommand.Property2"/>: Description of property 2 (adjust according to InvoicePaymentOptionCreateDto properties).
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{int}"/> representing the identifier of the created invoice payment option.
    /// </returns>
    /// <remarks>
    /// This method allows creating a new invoice payment option in the database.
    /// 
    /// Example request:
    /// 
    ///     POST /api/Create
    /// </remarks>
    /// <response code="200">If the request is successfully processed and the identifier of the created invoice payment option is returned.</response>
    /// <response code="400">If the request is invalid or required parameters are missing.</response>
    /// <response code="500">If an internal server error occurs.</response>            
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateInvoicePaymentOptionCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Updates an existing invoice payment option.
    /// </summary>
    /// <param name="id">
    /// Identifier of the invoice payment option to be updated.
    /// </param>
    /// <param name="command">
    /// Command containing the necessary parameters for the operation:
    /// - <see cref="UpdateInvoicePaymentOptionCommand.Id"/>: Identifier of the invoice payment option.
    /// - <see cref="UpdateInvoicePaymentOptionCommand.Property1"/>: Description of property 1 (adjust according to InvoicePaymentOptionUpdateDto properties).
    /// - <see cref="UpdateInvoicePaymentOptionCommand.Property2"/>: Description of property 2 (adjust according to InvoicePaymentOptionUpdateDto properties).
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult"/> indicating the result of the operation.
    /// </returns>
    /// <remarks>
    /// This method allows updating an existing invoice payment option in the database.
    /// 
    /// Example request:
    /// 
    ///     PUT /api/{id}
    /// </remarks>
    /// <response code="204">If the request is successfully processed and the invoice payment option is updated.</response>
    /// <response code="400">If the identifier does not match the command or the request is invalid.</response>
    /// <response code="404">If no invoice payment option is found for the specified identifier.</response>
    /// <response code="500">If an internal server error occurs.</response>            
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateInvoicePaymentOptionCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        await Mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Deletes an existing invoice payment option.
    /// </summary>
    /// <param name="id">
    /// Identifier of the invoice payment option to be deleted.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult"/> indicating the result of the operation.
    /// </returns>
    /// <remarks>
    /// This method allows deleting an invoice payment option from the database by marking it as deleted.
    /// 
    /// Example request:
    /// 
    ///     DELETE /api/{id}
    /// </remarks>
    /// <response code="204">If the request is successfully processed and the invoice payment option is deleted.</response>
    /// <response code="404">If no invoice payment option is found for the specified identifier.</response>
    /// <response code="500">If an internal server error occurs.</response>            
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteInvoicePaymentOptionCommand() { Id = id });

        return NoContent();
    }
}