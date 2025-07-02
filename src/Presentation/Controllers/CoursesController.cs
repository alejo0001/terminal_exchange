using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.JsonConverters;
using CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses.Commands;
using CrmAPI.Application.Courses.Queries.GetTopSellingByFacultiesAndCountry;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace CrmAPI.Presentation.Controllers;

public class CoursesController : ApiControllerBase
{
    private static readonly JsonSerializerOptions CustomJsonSerializerOptions =
        new() { Converters = { new DetailExceptionConverter() } };

    /// <summary>
    /// Obtains the top-selling courses based on faculties and country code.
    /// </summary>
    /// <param name="query">
    /// Query that contains the necessary parameters for the operation:
    /// - <see cref="GetTopSellingByFacultiesAndCountryQuery.CountryCode"/>: Country code.
    /// - <see cref="GetTopSellingByFacultiesAndCountryQuery.FacultiesId"/>: List of faculty identifiers (optional).
    /// - <see cref="GetTopSellingByFacultiesAndCountryQuery.Quantity"/>: Number of courses to return (optional).
    /// </param>
    /// <returns>
    /// A list of objects of type <see cref="TopSellingCourseDto"/> representing the found top-selling courses.
    /// </returns>
    /// <remarks>
    /// This method queries the database to retrieve the top-selling courses that match the provided country code and faculties.
    /// If no faculties are specified, it returns the top-selling courses for the indicated country.
    /// 
    /// Example request:
    /// 
    ///     GET /api/TopSellingByFacultiesAndCountry?CountryCode=ES&FacultiesId=1,2&Quantity=5
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the top-selling courses are returned.</response>
    /// <response code="404">If no top-selling courses are found for the specified parameters.</response>
    /// <response code="500">If an internal server error occurs.</response>    
    [HttpGet("TopSellingByFacultiesAndCountry")]
    public async Task<List<TopSellingCourseDto>> GetTopSellingByFacultiesAndCountry(
    [FromQuery] GetTopSellingByFacultiesAndCountryQuery query) =>
    await Mediator.Send(query).ConfigureAwait(false);

    /// <summary>
    /// Population of missing interested courses based on the top-selling courses.
    /// </summary>
    /// <param name="command">
    /// Command that contains the necessary parameters for the operation:
    /// - <see cref="PopulateMissingInterestedCoursesCommand.Area"/>: Area of interest.
    /// - <see cref="PopulateMissingInterestedCoursesCommand.CountryCode"/>: Country code.
    /// - <see cref="PopulateMissingInterestedCoursesCommand.ApiKey"/>: API key.
    /// - <see cref="PopulateMissingInterestedCoursesCommand.MaxJobContacts"/>: Maximum contacts to process (optional).
    /// - <see cref="PopulateMissingInterestedCoursesCommand.ContactIds"/>: Contact identifiers (optional).
    /// </param>
    /// <returns>
    /// An object of type <see cref="IActionResult"/> indicating the status of the operation.
    /// </returns>
    /// <remarks>
    ///     Note that this is a potentially long-running request and may result in a timeout during execution
    ///     (nature of HTTP requests).<br />
    ///     Note! A timeout on the HTTP request (e.g., connection closes or timeout expires) will not stop execution.
    ///     A log will always be recorded.
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the results of the operation are returned.</response>
    /// <response code="400">If the request is invalid or required parameters are missing.</response>    
    [HttpPost("populate-missing-interested-courses-by-top-sellers")]
    [SwaggerResponse(HttpStatusCode.OK, typeof(PopulateMissingInterestedCoursesResult))]
    [SwaggerResponse(HttpStatusCode.BadRequest, typeof(Error))]
    public async Task<IActionResult> PopulateMissingInterestedCoursesByTopSellers(
        PopulateMissingInterestedCoursesCommand command
        )
    {
        // CancellationToken.None is explicitly set to not initiate cancellation, when connection is timed out,
        // which is known to be happened as job might take longer than 2 min and some clients tend to timeout.
        var result = await Mediator.Send(command, CancellationToken.None);

        var httpResult = result.Match<IActionResult>(
            Ok,
            errors => errors.Any(e => e.Type is not ErrorType.Validation)
                ? base.StatusCode(500, TransformToProblem("Failure", errors))
                : BadRequest(TransformToProblem("Bad Request", errors))
        );

        return httpResult;
    }

    /// <summary>
    /// Population of missing interested courses based on the top-selling courses without waiting.
    /// </summary>
    /// <param name="command">
    /// Command that contains the necessary parameters for the operation:
    /// - <see cref="PopulateMissingInterestedCoursesCommand.Area"/>: Area of interest.
    /// - <see cref="PopulateMissingInterestedCoursesCommand.CountryCode"/>: Country code.
    /// - <see cref="PopulateMissingInterestedCoursesCommand.ApiKey"/>: API key.
    /// - <see cref="PopulateMissingInterestedCoursesCommand.MaxJobContacts"/>: Maximum contacts to process (optional).
    /// - <see cref="PopulateMissingInterestedCoursesCommand.ContactIds"/>: Contact identifiers (optional).
    /// </param>
    /// <returns>
    /// An object of type <see cref="IActionResult"/> indicating the status of the operation.
    /// </returns>
    /// <remarks>
    /// This method initiates a process to populate the missing interested courses based on the top-selling courses.
    /// The operation runs asynchronously and does not wait for completion.
    /// 
    /// Example request:
    /// 
    ///     POST /api/populate-missing-interested-courses-by-top-sellers-nowait
    /// </remarks>
    /// <response code="202">If the request is accepted and the process is started successfully.</response>
    /// <response code="400">If the request is invalid or required parameters are missing.</response>    
    [HttpPost("populate-missing-interested-courses-by-top-sellers-nowait")]
    [SwaggerResponse(HttpStatusCode.Accepted, typeof(string))]
    [SwaggerResponse(HttpStatusCode.BadRequest, typeof(Error))]
    public Task<IActionResult> PopulateMissingInterestedCoursesByTopSellersNoWait(
        PopulateMissingInterestedCoursesCommand command
    )
    {
        // CancellationToken.None is explicitly set to not initiate cancellation, when connection is timed out,
        // which is known to be happened as job might take longer than 2 min and some clients tend to timeout.
        Mediator.Send(command, CancellationToken.None);

        return Task.FromResult<IActionResult>(
            base.Accepted(value: "For result, see logs of SourceContext " +
                                 "`CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses`. "));
    }

    private static ProblemDetails TransformToProblem(string reason, List<Error> errors)
    {
        var problemDetails = new ProblemDetails
        {
            Title = reason,
            Detail = $"Errors on processing job `{nameof(PopulateMissingInterestedCoursesByTopSellers)}`",
        };

        var i = 1;
        foreach (var error in errors)
        {
            var metaData = error.Metadata?.ToDictionary(
                pair => pair.Key,
                pair => JsonSerializer.Serialize(pair.Value, CustomJsonSerializerOptions));

            problemDetails.Extensions[$"error-{i++}"] = new
            {
                error.Code,
                error.Description,
                Type = error.Type.ToString(),
                MetaData = metaData,
            };
        }

        return problemDetails;
    }
}
