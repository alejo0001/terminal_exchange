using System;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.UtcTimeZone.Queries;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class UtcTimeZoneController : ApiControllerBase
{
    /// <summary>
    /// Convierte una fecha y hora UTC a la zona horaria de un país específico.
    /// </summary>
    /// <param name="query">
    /// Parámetros de la solicitud, incluyendo el código ISO del país de destino y la fecha/hora en UTC a convertir.
    /// </param>
    /// <param name="ct">
    /// Token de cancelación para abortar la operación si es necesario.
    /// </param>
    /// <returns>
    /// La fecha y hora convertida a la zona horaria del país especificado en formato "yyyy-MM-dd HH:mm:ss".
    /// </returns>
    /// <response code="200">Devuelve la fecha y hora convertida exitosamente.</response>
    /// <response code="400">Si el código ISO del país no es válido o no se encuentra.</response>
    /// <response code="401">Si el usuario no está autenticado o autorizado para acceder al recurso.</response>
    /// <response code="500">Si ocurre un error inesperado durante la conversión.</response>
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
            return StatusCode(500, new { message = "Ocurrió un error interno en el servidor.", details = ex.Message });
        };
    }
}