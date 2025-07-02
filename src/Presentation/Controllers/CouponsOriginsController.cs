using System.Collections.Generic;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Faculties.Queries.GetCouponsOrigins;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class CouponsOriginsController : ApiControllerBase
{
    /// <summary>
    /// Obtains a list of coupon origins.
    /// </summary>
    /// <returns>
    /// A list of objects of type <see cref="CouponsOriginsDto"/> representing the found coupon origins.
    /// </returns>
    /// <remarks>
    /// This method queries the database to retrieve the list of coupon origins that are not marked as deleted.
    /// 
    /// Example request:
    /// 
    ///     GET /api/CouponsOrigins/List
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the coupon origins are returned.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("List")]
    public async Task<List<CouponsOriginsDto>> GetCouponsOrigins()
    {
        return await Mediator.Send(new GetCouponsOriginsQuery());
    }
}