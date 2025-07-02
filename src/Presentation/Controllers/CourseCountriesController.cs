using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.CourseCountries.Queries.GetCourseCountriesByCountryCodeAndLanguageCode;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class CourseCountriesController : ApiControllerBase
{
    /// <summary>
    /// Obtains a course of countries based on the country code and language code.
    /// </summary>
    /// <param name="query">
    /// Query that contains the necessary parameters for the operation:
    /// - <see cref="GetCourseCountriesByCountryCodeAndLanguageCodeQuery.CountryCode"/>: Country code.
    /// - <see cref="GetCourseCountriesByCountryCodeAndLanguageCodeQuery.LanguageCode"/>: Language code.
    /// </param>
    /// <returns>
    /// An object of type <see cref="CourseCountryDto"/> representing the found country course.
    /// </returns>
    /// <remarks>
    /// This method queries the database to retrieve the country course that matches the provided country code and language code.
    /// If the country course is not found, it looks for the course of Spain with the default language.
    /// 
    /// Example request:
    /// 
    ///     GET /api/CourseCountries?CountryCode=ES&LanguageCode=ES
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the country course is returned.</response>
    /// <response code="404">If the specified country course is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>    
    [HttpGet]
    public async Task<ActionResult<CourseCountryDto>> GetCourseCountriesByCountryCodeAndLanguageCode(
        [FromQuery] GetCourseCountriesByCountryCodeAndLanguageCodeQuery query)
    {
        return await Mediator.Send(query);
    }
}