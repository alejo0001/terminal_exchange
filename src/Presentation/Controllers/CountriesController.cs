using System.Collections.Generic;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Countries.Queries.GetCountries;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class CountriesController : ApiControllerBase
{
    /// <summary>
    /// Obtains a list of countries.
    /// </summary>
    /// <returns>
    /// A list of objects of type <see cref="CountryDto"/> representing the found countries.
    /// </returns>
    /// <remarks>
    /// This method queries the database to retrieve the list of countries.
    /// 
    /// Example request:
    /// 
    ///     GET /api/Countries/List
    /// 
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the countries are returned.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("List")]
    public async Task<List<CountryDto>> GetCountries()
    {
        return await Mediator.Send(new GetCountriesQuery());
    }
    // test lentitud
}