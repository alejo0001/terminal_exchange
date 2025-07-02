using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Currencies.Queries.GetCurrencyByCode;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class CurrenciesController : ApiControllerBase
{
    /// <summary>
    /// Obtains the currency information based on the currency code.
    /// </summary>
    /// <param name="query">
    /// Query that contains the necessary parameters for the operation:
    /// - <see cref="GetCurrencyByCodeQuery.CurrencyCode"/>: Currency code.
    /// </param>
    /// <returns>
    /// An object of type <see cref="CurrencyDto"/> representing the found currency information.
    /// </returns>
    /// <remarks>
    /// This method queries the database to retrieve the currency information that matches the provided currency code.
    /// 
    /// Example request:
    /// 
    ///     GET /api/CurrencyByCode?CurrencyCode=USD
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the currency information is returned.</response>
    /// <response code="404">If the currency for the specified code is not found.</response>
    /// <response code="500">If an internal server error occurs.</response> 
    [HttpGet("CurrencyByCode")]
    public async Task<CurrencyDto> GetCurrencyByCode(
        [FromQuery] GetCurrencyByCodeQuery query)
    {
        return await Mediator.Send(query);
    }
}