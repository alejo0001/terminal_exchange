using System.Collections.Generic;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Models;
using CrmAPI.Application.OrdersImported.Commands.CreateOrdersImportedFromTlmk;
using CrmAPI.Application.OrdersImported.Commands.DeleteOrderImportedFromTlmkByEmail;
using CrmAPI.Application.OrdersImported.Commands.SetProcessStatusByOrderNumber;
using CrmAPI.Application.OrdersImported.Queries.GetOrdersImportedByContact;
using CrmAPI.Application.OrdersImported.Queries.GetOrdersImportedByContactWithPagination;
using CrmAPI.Application.OrdersImported.Queries.GetOrdersImportedDetails;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class OrdersImportedController: ApiControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of imported orders associated with a specific contact.
    /// </summary>
    /// <param name="query">
    /// Query containing the parameters required for the operation:
    /// - <see cref="GetOrdersImportedByContactWithPaginationQuery.ContactId"/>: Identifier of the contact.
    /// - <see cref="GetOrdersImportedByContactWithPaginationQuery.PageNumber"/>: Page number for pagination (optional, default is 1).
    /// - <see cref="GetOrdersImportedByContactWithPaginationQuery.PageSize"/>: Page size for pagination (optional, default is 25).
    /// - <see cref="GetOrdersImportedByContactWithPaginationQuery.QueryParams"/>: Filtering parameters for the results (optional).
    /// - <see cref="GetOrdersImportedByContactWithPaginationQuery.OrderBy"/>: Fields for sorting the results (optional, default is "Id").
    /// - <see cref="GetOrdersImportedByContactWithPaginationQuery.Order"/>: Order direction (optional, default is "asc").
    /// </param>
    /// <returns>
    /// A paginated list of <see cref="OrdersImportedDto"/> representing the imported orders associated with the contact.
    /// </returns>
    /// <remarks>
    /// This method retrieves imported orders for a given contact, optionally applying filters and sorting based on the provided parameters.
    /// 
    /// Example request:
    /// 
    ///     GET /api/paginated?ContactId=1&PageNumber=1&PageSize=25&OrderBy=Name&Order=desc
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the list of imported orders is returned.</response>
    /// <response code="404">If no imported orders are found for the specified contact.</response>
    /// <response code="500">If an internal server error occurs.</response>    
    [HttpGet("paginated")]
    public async Task<ActionResult<PaginatedList<OrdersImportedDto>>> GetOrdersImportedByContact(
        [FromQuery] GetOrdersImportedByContactWithPaginationQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Retrieves a list of imported orders associated with a specific contact.
    /// </summary>
    /// <param name="query">The query parameters containing the contact ID and an optional list of contact emails.</param>
    /// <returns>A list of imported orders related to the contact.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     GET /api/orders-imported?ContactId=1&ContactEmails=email1@example.com,email2@example.com   
    /// 
    /// <response code="200">Returns the list of imported orders for the specified contact.</response>
    /// <response code="400">If the query parameters are invalid.</response>
    /// <response code="401">If the user is not authorized to access this endpoint.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpGet]
    public async Task<ActionResult<List<OrdersImportedDto>>> GetOrdersImportedByContact(
        [FromQuery] GetOrdersImportedByContactQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Retrieves the details of a specific imported order by its ID.
    /// </summary>
    /// <param name="id">The ID of the imported order to retrieve.</param>
    /// <returns>The details of the imported order.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     GET /api/orders-imported/{id}
    ///
    /// Returns the details of the imported order corresponding to the provided ID.
    /// 
    /// <response code="200">Returns the details of the specified imported order.</response>
    /// <response code="404">If no imported order is found with the specified ID.</response>
    /// <response code="401">If the user is not authorized to access this endpoint.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>            
    [HttpGet("{id}")]
    public async Task<ActionResult<OrdersImportedDto>> GetDetails(int id)
    {
        return await Mediator.Send(new GetOrdersImportedDetailsQuery(){ Id = id });
    }

    /// <summary>
    /// Creates and saves a new imported order from TLMK.
    /// </summary>
    /// <param name="command">The command containing the information of the order to be imported from TLMK.</param>
    /// <returns>The ID of the created imported order.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     POST /api/orders-imported/Tlmk
    ///     {
    ///         "ApiKey": "api-key",
    ///         "NumPedido": "12345",
    ///         "Nombre": "Juan Pérez",
    ///         "Email": "juan.perez@example.com",
    ///         "Titulo": "Programming Course"    
    ///     }
    ///
    /// <response code="200">Returns the ID of the created imported order.</response>
    /// <response code="400">If the order data is invalid.</response>
    /// <response code="401">If the user is not authorized to perform this operation.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpPost("Tlmk")]
    public async Task<ActionResult<int>> SaveOrderFromTlmk(CreateOrdersImportedFromTlmkCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Updates the status of a process based on the order number.
    /// </summary>
    /// <param name="command">The command containing the order number, the new process status, the outcome, and the payment type.</param>
    /// <returns>A string indicating the result of the operation.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     PUT /api/orders-imported/SetProcessStatusByOrderNumber
    ///     {
    ///         "OrderNumber": 12345,
    ///         "Status": "Completed",
    ///         "Outcome": "Success",
    ///         "PaymentType": "CreditCard"
    ///     }
    ///
    /// <response code="200">Returns a string indicating that the operation was successful.</response>
    /// <response code="400">If the provided data is invalid.</response>
    /// <response code="404">If no order or process associated with the specified order number is found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpPut("SetProcessStatusByOrderNumber")]
    public async Task<ActionResult<string>> SetProcessByOrderNumber(SetProcessStatusByOrderNumberCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Deletes imported orders associated with a list of email addresses.
    /// </summary>
    /// <param name="command">The command containing the list of email addresses associated with the orders to be deleted.</param>
    /// <returns>A list of IDs of the deleted orders.</returns>
    /// <remarks>
    /// Example request:
    ///
    ///     DELETE /api/orders-imported/DeleteOrderImportedFromTlmkByEmail
    ///     {
    ///         "Emails": ["email1@example.com", "email2@example.com"]
    ///     }
    ///
    /// <response code="200">Returns a list of IDs of the deleted orders.</response>
    /// <response code="400">If the provided data is invalid.</response>
    /// <response code="401">If the user is not authorized to perform this operation.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// </remarks>    
    [HttpDelete("DeleteOrderImportedFromTlmkByEmail")]
    public async Task<ActionResult<List<int>>> DeleteOrderImportedFromTlmkByEmail(DeleteOrderImportedFromTlmkByEmailCommand command)
    {
        return await Mediator.Send(command);
    }
}