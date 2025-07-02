using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Models;
using CrmAPI.Application.DiscardReasons.Queries.GetAllDiscardReasons;
using CrmAPI.Application.Processes.Commands.CloseProcessByOrderEmailOrPhone;
using CrmAPI.Application.Processes.Commands.CloseProcesses;
using CrmAPI.Application.Processes.Commands.CreateProcess;
using CrmAPI.Application.Processes.Commands.DeleteProcess;
using CrmAPI.Application.Processes.Commands.ExternalSuccessfulSaleProcess;
using CrmAPI.Application.Processes.Commands.ReassignAllUserProcesses;
using CrmAPI.Application.Processes.Commands.ReassignProcess;
using CrmAPI.Application.Processes.Commands.ReplaceProcessForPriorityCommercial;
using CrmAPI.Application.Processes.Commands.ReturnBusinessContactsToDelivery;
using CrmAPI.Application.Processes.Commands.SetWebSaleProcess;
using CrmAPI.Application.Processes.Commands.UpdateProcess;
using CrmAPI.Application.Processes.Commands.UpdateProcessColor;
using CrmAPI.Application.Processes.Queries.GetCanRecoverProcesses;
using CrmAPI.Application.Processes.Queries.GetOpenProcessesByContactId;
using CrmAPI.Application.Processes.Queries.GetProcessesInProgressByUserWithPagination;
using CrmAPI.Application.Processes.Queries.GetProcessesNotSalesByUserWithPagination;
using CrmAPI.Application.Processes.Queries.GetProcessPendingByUserWithPagination;
using CrmAPI.Application.Processes.Queries.GetProcessSaleAttemtps;
using CrmAPI.Application.Processes.Queries.GetProcessSaleStatus;
using CrmAPI.Application.Processes.Queries.GetSalesByUserWithPagination;
using CrmAPI.Application.Processes.Queries.GetSuggestedNextInteractionDate;
using CrmAPI.Application.Processes.Queries.GetTypesActiveProcesses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class ProcessesController : ApiControllerBase
{
    #region Get
    /// <summary>
    /// Retrieves a list of processes that are "in progress" and related to the authenticated user.
    /// </summary>
    /// <param name="query">
    /// Object containing query parameters, including pagination options and filters 
    /// (such as email, phone, country code).
    /// </param>
    /// <returns>
    /// A <see cref="PaginatedList{ProcessDto}"/> object containing the paginated list of processes in progress.
    /// </returns>
    [HttpGet("InProgress")]
    public async Task<ActionResult<PaginatedList<ProcessInProgressDto>>> GetProcessesInProgressByUser(
        [FromQuery] GetProcessesInProgressByUserWithPaginationQuery query) =>
        await Mediator.Send(query);

    /// <summary>
    /// Retrieves a list of processes that are "in progress" and associated with a specific user.
    /// </summary>
    /// <param name="query">
    /// Object containing query parameters for the search, including filters and pagination options.
    /// </param>
    /// <returns>
    /// A paginated list of in-progress processes as <see cref="PaginatedList{ProcessInProgressDto}"/>.
    /// </returns>
    [HttpGet("InProgress2")]
    public async Task<ActionResult<PaginatedList<ProcessDto>>> GetProcessesInProgressByUser(
        [FromQuery] GetProcessesInProgressByUserWithPaginationQueryPocho query) =>
        await Mediator.Send(query);

    /// <summary>
    /// Retrieves a paginated list of closed processes that did not result in a sale and are associated with the authenticated user.
    /// </summary>
    /// <param name="query">
    /// Object containing the query parameters.
    /// </param>
    /// <returns>
    /// A <see cref="PaginatedList{ProcessDto}"/> object containing the paginated list of closed processes without a sale 
    /// associated with the authenticated user.
    /// </returns>
    [HttpGet("NotSales")]
    public async Task<ActionResult<PaginatedList<ProcessDto>>> GetProcessesNotSalesByUser(
        [FromQuery] GetProcessesNotSalesByUserWithPaginationQuery query) =>
        await Mediator.Send(query);

    /// <summary>
    /// Retrieves a list of processes managed by a specific user, filtered by an optional sales type.
    /// </summary>
    /// <param name="query">
    /// Object containing the required parameters:
    /// - <see cref="GetProcessesSalesByUserQuery.UserId"/>: ID of the user managing the processes.
    /// - <see cref="GetProcessesSalesByUserQuery.SalesTypeId"/>: (Optional) ID of the sales type to filter by.
    /// </param>
    /// <returns>
    /// A list of <see cref="ProcessSalesDto"/> objects representing the sales processes managed by the specified user.
    /// </returns>
    [HttpGet("Sales")]
    public async Task<ActionResult<PaginatedList<ProcessDto>>> GetProcessesSalesByUser(
        [FromQuery] GetProcessesSalesByUserWithPaginationQuery query) =>
        await Mediator.Send(query);

    /// <summary>
    /// Retrieves a paginated list of pending processes associated with a specific user.
    /// </summary>
    /// <param name="query">
    /// Object containing the query parameters.
    /// </param>
    /// <returns>
    /// A <see cref="PaginatedList{ProcessDto}"/> object containing the paginated list of pending processes
    /// associated with the specified user.
    /// </returns>
    [HttpGet("Pending")]
    public async Task<ActionResult<PaginatedList<ProcessDto>>> GetProcessesPendingByUser(
        [FromQuery] GetProcessPendingByUserWithPaginationQuery query) =>
        await Mediator.Send(query);

    /// <summary>
    /// Retrieves the active process types associated with the current user.
    /// </summary>
    /// <returns>
    /// A list of <see cref="ProcessTypeDto"/> objects representing the currently active (ongoing or pending) process types for the user.
    /// </returns>
    [HttpGet("Types/ActiveProcesses")]
    public async Task<ActionResult<List<ProcessTypeDto>>> GetTypesActiveProcesses() =>
        await Mediator.Send(new GetTypesActiveProcessesQuery());

    /// <summary>
    /// Retrieves the sales status of a specific process.
    /// </summary>
    /// <param name="processId">
    /// Identifier of the process for which the sales status is to be obtained.
    /// </param>
    /// <returns>
    /// A <see cref="ProcessSaleStatusDto"/> object containing the sales status of the process.
    /// - <see cref="ProcessSaleStatusDto.SaleComplete"/>: Indicates whether the sale is complete.
    /// </returns>
    [HttpGet("SaleStatus/{processId}")]
    public async Task<ProcessSaleStatusDto> GetProcessSaleStatus(int processId) =>
        await Mediator.Send(new GetProcessSaleStatusQuery { ProcessId = processId });

    /// <summary>
    /// Retrieves the number of sales attempts for a specific process.
    /// </summary>
    /// <param name="processId">
    /// Identifier of the process for which the number of sales attempts is to be obtained.
    /// </param>
    /// <returns>
    /// A <see cref="ProcessSaleAttemtpsDto"/> object containing the number of sales attempts for the process.
    /// - <see cref="ProcessSaleAttemtpsDto.SaleAttemtps"/>: Number of sales attempts recorded for the process.
    /// </returns>
    [HttpGet("SaleAttemtps/{processId}")]
    public async Task<ProcessSaleAttemtpsDto> GetProcessSaleAttempts(int processId) =>
        await Mediator.Send(new GetProcessSaleAttemtpsQuery { ProcessId = processId });

    /// <summary>
    /// Checks if the current user can create a new recovery process.
    /// </summary>
    /// <returns>
    /// A <see cref="ActionResult{bool}"/> indicating whether the user can create a recovery process.
    /// - <c>true</c> if the user can create a new recovery process; otherwise, <c>false</c>.
    /// </returns>
    [HttpGet("CanCreateRecoverProcess")]
    public async Task<ActionResult<bool>> CanCreateRecoverProcess() =>
        await Mediator.Send(new GetCanCreateRecoverProcessQuery());

    /// <summary>
    /// Retrieves a list of all available discard reasons.
    /// </summary>
    /// <returns>
    /// A list of <see cref="DiscardReasonDto"/> objects representing the discard reasons.
    /// </returns>
    [HttpGet("DiscardReasons/List")]
    public async Task<List<DiscardReasonDto>> GetAllDiscardReasons() =>
        await Mediator.Send(new GetAllDiscardReasonsQuery());

    /// <summary>
    /// Retrieves a list of open processes associated with a specific contact.
    /// </summary>
    /// <param name="contactId">
    /// Identifier of the contact for whom the open processes are to be retrieved.
    /// </param>
    /// <returns>
    /// A list of <see cref="ProcessDto"/> objects representing the open processes.
    /// </returns>
    [HttpGet("GetOpenProcessesByContactId/{contactId}")]
    public async Task<List<ProcessDto>> GetOpenProcessesByContactId(int contactId) =>
        await Mediator.Send(
            new GetOpenProcessesByContactIdQuery
            {
                ContactId = contactId,
            });

    /// <summary>
    /// Retrieves the suggested date for the next interaction based on the process and the employee's local date.
    /// </summary>
    /// <param name="processId">
    /// Identifier of the process for which the suggested interaction date is to be obtained.
    /// </param>
    /// <param name="dateLocalEmployee">
    /// Employee's local date and time in string format, without a specified time zone. 
    /// Expected format: 2024-09-03T09:34:56.789
    /// </param>
    /// <param name="ct">Cancellation token for the asynchronous operation.</param>
    /// <returns>
    /// A suggested date and time for the next interaction, or <c>null</c> if it cannot be determined.
    /// </returns>
    [Description("Date Local Time with unspecified type Zone (without HourZone). Expected: 2024-09-03T09:00:00.000")]
    [HttpGet("GetSuggestedNextInteractionDate/{processId}/{dateLocalEmployee}")]
    public async Task<DateTime?> GetSuggestedNextInteractionDate(
        int processId,
        string dateLocalEmployee,
        CancellationToken ct) =>
        await Mediator.Send(new GetSuggestedNextInteractionDateQuery(processId, dateLocalEmployee), ct);
    #endregion

    #region Post
    /// <summary>
    /// Creates a new process.
    /// </summary>
    /// <param name="command">
    /// Object containing the necessary data for creating the process.
    /// </param>
    /// <returns>
    /// The unique identifier (<see cref="int"/>) of the newly created process.
    /// </returns>
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateProcessCommand command) => await Mediator.Send(command);

    /// <summary>
    /// Sets a web sales process using the contact's email, phone, or ID.
    /// </summary>
    /// <param name="command">
    /// Object containing the parameters required to set the sales process:
    /// - <see cref="SetWebSaleProcessByEmailOrPhoneOrDniCommand.Email"/>: Contact's email address.
    /// - <see cref="SetWebSaleProcessByEmailOrPhoneOrDniCommand.Phone"/>: Contact's phone number.
    /// - <see cref="SetWebSaleProcessByEmailOrPhoneOrDniCommand.Dni"/>: Contact's ID.
    /// </param>
    /// <returns>
    /// A <see cref="Unit"/> object indicating the operation was completed successfully.
    /// </returns>
    [HttpPost("SetWebSaleProcessByEmailOrPhoneOrDniCommand")]
    public async Task<Unit>
        SaleProcessByEmailOrPhoneOrDniCommand(SetWebSaleProcessByEmailOrPhoneOrDniCommand command) =>
        await Mediator.Send(command);
    #endregion

    #region Put
    /// <summary>
    /// Updates the data of an existing process.
    /// </summary>
    /// <param name="id">
    /// The unique identifier (<see cref="int"/>) of the process to be updated.
    /// </param>
    /// <param name="command">
    /// Object containing the updated data for the process. This command inherits from <see cref="ProcessUpdateDto"/>.
    /// </param>
    /// <returns>
    /// Returns an HTTP result:
    /// - <see cref="BadRequest"/> if the identifier provided in the parameter does not match the ID in the command.
    /// - <see cref="NoContent"/> if the update is successfully completed.
    /// </returns>
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateProcessCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        await Mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Changes the color associated with a specific process.
    /// </summary>
    /// <param name="command">
    /// Object of type <see cref="UpdateProcessColorCommand"/> containing:
    /// - <see cref="UpdateProcessColorCommand.ProcessId"/>: Unique ID of the process to update.
    /// - <see cref="UpdateProcessColorCommand.Color"/>: New color to assign to the process.
    /// </param>
    /// <returns>
    /// An HTTP response with:
    /// - <see cref="NoContent"/> if the operation is successfully completed.
    /// </returns>
    [HttpPut("ChangeProcessColor")]
    public async Task<ActionResult> ChangeProcessColor([FromQuery] UpdateProcessColorCommand command)
    {
        await Mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Closes pending sales processes based on the associated order number or email address.
    /// </summary>
    /// <param name="command">
    /// Object of type <see cref="CloseProcessSaleByOrderNumberOrEmailCommand"/> containing a list of 
    /// <see cref="OrderImportedUpdateDto"/> objects with the following information:
    /// - <see cref="OrderImportedUpdateDto.OrderNumber"/>: Order number associated with the process.
    /// - <see cref="OrderImportedUpdateDto.PaymentType"/>: Type of payment made.
    /// - <see cref="OrderImportedUpdateDto.PaymentDate"/>: Date of the payment.
    /// - <see cref="OrderImportedUpdateDto.Status"/>: Order status (e.g., "CANCELLED").
    /// </param>
    /// <returns>
    /// An HTTP response with:
    /// - <see cref="NoContent"/> if the operation is successfully completed.
    /// </returns>
    [HttpPut("CloseProcessSaleByOrderNumberOrEmail")]
    public async Task<ActionResult> CloseProcessSaleByOrderNumberOrEmail(
        [FromBody] CloseProcessSaleByOrderNumberOrEmailCommand command)
    {
        await Mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Reassigns a process to a different user.
    /// </summary>
    /// <param name="command">
    /// Object containing the data necessary for the reassignment:
    /// - <see cref="ReassignProcessCommand.ProcessId"/>: Unique identifier of the process.
    /// - <see cref="ReassignProcessCommand.UserId"/>: Identifier of the user to whom the process will be reassigned.
    /// </param>
    /// <returns>
    /// An HTTP response with:
    /// - <see cref="NoContent"/> if the operation is successfully completed.
    /// </returns>
    [HttpPut("Reassign")]
    public async Task<ActionResult> ReassignProcess(ReassignProcessCommand command)
    {
        await Mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Reassigns all processes from one user to another user.
    /// </summary>
    /// <param name="command">
    /// Object containing the parameters necessary for the reassignment:
    /// - <see cref="ReassignAllUserProcessesCommand.FromUserId"/>: Identifier of the user from whom the processes will be reassigned.
    /// - <see cref="ReassignAllUserProcessesCommand.ToUserId"/>: Identifier of the user to whom the processes will be assigned.
    /// - <see cref="ReassignAllUserProcessesCommand.OnlyToDo"/>: Optional flag to reassign only processes with "To Do" status.
    /// </param>
    /// <returns>
    /// An HTTP response:
    /// - <see cref="NoContent"/> if the operation is successful.
    /// </returns>
    [HttpPut("ReassignAllUserProcesses")]
    public async Task<ActionResult> ReassignAllProcesses([FromQuery] ReassignAllUserProcessesCommand command)
    {
        await Mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Processes a successful external sale and updates the associated contact and process information.
    /// </summary>
    /// <param name="command">
    /// Object containing the parameters required to process the successful external sale:
    /// - <see cref="ExternalSuccessfulSaleProcessCommand.ProcessId"/>: Identifier of the associated process.
    /// - <see cref="ExternalSuccessfulSaleProcessCommand.IdCard"/>: Contact's ID.
    /// - <see cref="ExternalSuccessfulSaleProcessCommand.Email"/>: Contact's email address.
    /// - <see cref="ExternalSuccessfulSaleProcessCommand.Address"/>: Contact's address.
    /// - <see cref="ExternalSuccessfulSaleProcessCommand.PostalCode"/>: Postal code of the address.
    /// - <see cref="ExternalSuccessfulSaleProcessCommand.Country"/>: Country code.
    /// - <see cref="ExternalSuccessfulSaleProcessCommand.Province"/>: Province of the address.
    /// - <see cref="ExternalSuccessfulSaleProcessCommand.City"/>: City of the address.
    /// - <see cref="ExternalSuccessfulSaleProcessCommand.OrderNumber"/>: Order number.
    /// - <see cref="ExternalSuccessfulSaleProcessCommand.OrderDate"/>: Order date.
    /// - <see cref="ExternalSuccessfulSaleProcessCommand.AcademicTitle"/>: Academic title.
    /// - <see cref="ExternalSuccessfulSaleProcessCommand.InitDate"/>: Start date.
    /// - <see cref="ExternalSuccessfulSaleProcessCommand.PaymentType"/>: Payment type.
    /// - <see cref="ExternalSuccessfulSaleProcessCommand.CurrencyCountry"/>: Currency country.
    /// - <see cref="ExternalSuccessfulSaleProcessCommand.NumberDeadLines"/>: Number of installments.
    /// - <see cref="ExternalSuccessfulSaleProcessCommand.SalesCountry"/>: Sales country.
    /// - <see cref="ExternalSuccessfulSaleProcessCommand.AmountRegistration"/>: Registration amount.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult"/> object indicating the operation's result.
    /// - <see cref="NoContent"/> if the operation was completed successfully.
    /// </returns>
    [HttpPut("ExternalSuccessfulSaleProcess")]
    public async Task<ActionResult> ExternalSuccessfulSaleProcessCommand(ExternalSuccessfulSaleProcessCommand command)
    {
        await Mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Closes the processes specified in the list of process identifiers.
    /// </summary>
    /// <param name="command">
    /// Object containing the parameters required to close the processes:
    /// - <see cref="CloseProcessesCommand.ProcessIds"/>: List of process identifiers to close.
    /// - <see cref="CloseProcessesCommand.IsAutomatic"/>: Indicates if the process closure is automatic (optional).
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult"/> object indicating the operation's result.
    /// - <see cref="NoContent"/> if the operation was completed successfully.
    /// </returns>
    [HttpPut("CloseProcesses")]
    public async Task<ActionResult> CloseProcessesCommand([FromQuery] CloseProcessesCommand command)
    {
        await Mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Replaces an existing process with a new one for a priority commercial user.
    /// </summary>
    /// <param name="command">
    /// Object containing the parameters required to replace the process:
    /// - <see cref="ReplaceProcessForPriorityCommercialCommand.ProcessId"/>: Identifier of the process to be replaced.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{int}"/> object containing the identifier of the newly created process.
    /// </returns>
    [HttpPut("ReplaceProcessForPriorityCommercial")]
    public async Task<ActionResult<int>> ReplaceProcessForPriorityCommercial(
        [FromQuery] ReplaceProcessForPriorityCommercialCommand command) =>
        await Mediator.Send(command);

    /// <summary>
    /// Devuelve los contactos comerciales para entrega basados en el correo electrónico corporativo proporcionado.
    /// </summary>
    /// <param name="command">Comando que contiene la ApiKey y el correo electrónico corporativo.</param>
    /// <returns>
    /// Un mensaje que indica que las fechas de reparto y el estado de los procesos se han actualizado correctamente.
    /// </returns>
    /// <remarks>
    /// Este método realiza las siguientes operaciones:
    /// 1. Obtiene el ID del usuario a partir del correo electrónico corporativo.
    /// 2. Obtiene los IDs de contacto asociados al usuario.
    /// 3. Actualiza la fecha de reparto de los contactos.
    /// 4. Actualiza el estado de los procesos.
    /// </remarks>
    /// <response code="200">Las fechas de reparto y el estado de los procesos se actualizaron correctamente.</response>
    /// <response code="400">Si el comando es nulo o no válido.</response>
    /// <response code="500">Si ocurre un error interno en el servidor.</response>
    [HttpPut("ReturnBusinnesContactsDelivery")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<string> ReturnBusinnesContactsDelivery([FromQuery] ReturnBusinessContactsToDeliveryCommands command)
        => await Mediator.Send(command);
    #endregion

    #region Delete
    /// <summary>
    /// Marks a process as deleted by its identifier.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the process to delete.
    /// </param>
    /// <returns>
    /// An HTTP response with:
    /// - <see cref="NoContent"/> if the operation is successfully completed.
    /// </returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteProcessCommand { Id = id });

        return NoContent();
    }
    #endregion        
}
