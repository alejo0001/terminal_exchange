using System.Collections.Generic;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Languages.Queries;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class LanguagesController: ApiControllerBase
{
    /// <summary>
    /// Retrieves the list of available languages.
    /// </summary>
    /// <returns>
    /// A list of <see cref="LanguageDto"/> objects representing the available languages.
    /// </returns>
    /// <remarks>
    /// This method retrieves all registered languages.
    /// 
    /// Example request:
    /// 
    ///     GET /api/GetLanguages
    /// </remarks>
    /// <response code="200">If the request is successfully processed and the list of languages is returned.</response>
    /// <response code="500">If an internal server error occurs.</response> 
    [HttpGet]
    public async Task<List<LanguageDto>> GetLanguages()
    {
        return await Mediator.Send(new GetLanguagesQuery());
    }
}