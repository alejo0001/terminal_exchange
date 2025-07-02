using System.Threading.Tasks;
using CrmAPI.Application.Users.Queries.GetUserSellerId;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class UsersController : ApiControllerBase
{
    /// <summary>
    /// Retrieves the ID of the seller associated with the current authenticated user.
    /// </summary>
    /// <remarks>
    /// This endpoint obtains the seller ID (`IdVendedor`) linked to the email 
    /// of the authenticated user.    
    ///
    /// **Example request:**
    ///     GET /api/users/GetCurrentUser SellerId
    ///     Headers:
    ///         Authorization: Bearer {token}
    ///
    /// **Expected responses:**
    /// - `200 OK`: Returns the associated seller ID.
    /// - `401 Unauthorized`: If the user is not authenticated.
    /// - `500 Internal Server Error`: If an internal error occurred.
    ///
    /// </remarks>
    /// <response code="200">Returns the ID of the seller associated with the user.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="500">Internal error while processing the request.</response>
    [HttpGet]
    public async Task<ActionResult<int?>> GetCurrentUserSellerId()
    {
        return await Mediator.Send(new GetCurrentUserSellerIdQuery());
    }
}