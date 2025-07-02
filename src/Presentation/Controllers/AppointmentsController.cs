using System.Threading.Tasks;
using CrmAPI.Application.Appointments.Commands.CreateAppointment;
using CrmAPI.Application.Appointments.Commands.DeleteAppointment;
using CrmAPI.Application.Appointments.Commands.UpdateAppointment;
using CrmAPI.Application.Appointments.Queries.GetAppointmentsByProcess;
using CrmAPI.Application.Common.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class AppointmentsController : ApiControllerBase
{
    /// <summary>
    /// Retrieves appointments associated with a specific process.
    /// </summary>
    /// <param name="processId">
    /// Unique identifier of the process for which appointments are to be retrieved.
    /// </param>
    /// <returns>
    /// An HTTP response with:
    /// - <see cref="AppointmentDto"/>: Object containing the appointment details (title, date, type).
    /// </returns>  
    [HttpGet("GetAppointmentsByProcess/{processId}")]
    public async Task<ActionResult<AppointmentDto>> GetAppointmentsByProcess(int processId)
    {
        return await Mediator.Send(new GetAppointmentsByProcessQuery
        {
            ProcessId = processId
        });
    }

    /// <summary>
    /// Creates a new appointment associated with a specific contact and process.
    /// </summary>
    /// <param name="command">
    /// Command containing the necessary data to create the appointment:
    /// - <see cref="CreateAppointmentCommand"/>: DTO with the appointment details.
    /// </param>
    /// <returns>
    /// An HTTP response with:
    /// - Unique identifier of the created appointment.
    /// </returns>
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateAppointmentCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Updates the details of an existing appointment.
    /// </summary>
    /// <param name="id">
    /// Unique identifier of the appointment to be updated.
    /// </param>
    /// <param name="command">
    /// Command with the new appointment data:
    /// - <see cref="UpdateAppointmentCommand"/>: DTO with the updated information.
    /// </param>
    /// <returns>
    /// An HTTP response with:
    /// - <see cref="NoContent"/> if the operation is successful.
    /// </returns>
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateAppointmentCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        await Mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Deletes an existing appointment.
    /// </summary>
    /// <param name="id">
    /// Unique identifier of the appointment to be deleted.
    /// </param>
    /// <returns>
    /// An HTTP response with:
    /// - <see cref="NoContent"/> if the operation is successful.
    /// </returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteAppointmentCommand() { Id = id });

        return NoContent();
    }
}