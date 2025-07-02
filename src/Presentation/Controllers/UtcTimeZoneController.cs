using System;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.UtcTimeZone.Queries;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class UtcTimeZoneController : ApiControllerBase
{
    /// <summary>
    /// Convierte una fecha y hora UTC a la zona horaria de un pa�s espec�fico.
    /// </summary>
    /// <param name="query">
    /// Par�metros de la solicitud, incluyendo el c�digo ISO del pa�s de destino y la fecha/hora en UTC a convertir.
    /// </param>
    /// <param name="ct">
    /// Token de cancelaci�n para abortar la operaci�n si es necesario.
    /// </param>
    /// <returns>
    /// La fecha y hora convertida a la zona horaria del pa�s especificado en formato "yyyy-MM-dd HH:mm:ss".
    /// </returns>
    /// <response code="200">Devuelve la fecha y hora convertida exitosamente.</response>
    /// <response code="400">Si el c�digo ISO del pa�s no es v�lido o no se encuentra.</response>
    /// <response code="401">Si el usuario no est� autenticado o autorizado para acceder al recurso.</response>
    /// <response code="500">Si ocurre un error inesperado durante la conversi�n.</response>
    [HttpGet("utc")]     
    public async Task<ActionResult<string>> UtcTimeZone([FromQuery] GetUtcTimeZoneQuery query, CancellationToken ct)
    {
        try
        {
            var result = await Mediator.Send(query, ct);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ocurri� un error interno en el servidor.", details = ex.Message });
        };
    }
}