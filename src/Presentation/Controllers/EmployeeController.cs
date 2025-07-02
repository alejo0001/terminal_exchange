using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Employees.Queries.GetAllManagerSubordinates;
using CrmAPI.Application.Employees.Queries.GetLoggedEmployee;
using CrmAPI.Application.Employees.Queries.GetManagerByEmployee;
using CrmAPI.Application.Employees.Queries.GetSignatureQuery;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class EmployeeController : ApiControllerBase
{
    /// <summary>
    /// Retrieves all subordinates of a manager.
    /// </summary>
    /// <param name="query">
    /// Query containing the necessary parameters for the operation (currently no parameters are required).
    /// </param>
    /// <returns>
    /// A list of <see cref="EmployeeSubordinateViewModel"/> objects representing the manager's subordinates.
    /// </returns>
    /// <remarks>
    /// This method queries the API to fetch all subordinates of the current manager.
    /// 
    /// Example request:
    /// 
    ///     GET /api/GetAllManagerSubordinates
    /// </remarks>
    /// <response code="200">If the request is successfully processed and the manager's subordinates are returned.</response>
    /// <response code="500">If an internal server error occurs.</response>        
    [HttpGet("GetAllManagerSubordinates")]
    public async Task<List<EmployeeSubordinateViewModel>> GetAllManagerSubordinates(
        [FromQuery] GetAllManagerSubordinatesQuery query) =>
        await Mediator.Send(query);

    /// <summary>
    /// Retrieves the manager associated with an employee.
    /// </summary>
    /// <param name="query">
    /// Query containing the necessary parameters for the operation (currently no parameters are required).
    /// </param>
    /// <returns>
    /// An object of type <see cref="ManagerDto"/> representing the manager associated with the employee.
    /// </returns>
    /// <remarks>
    /// This method queries the database to fetch the manager of the current employee based on their corporate email.
    /// 
    /// Example request:
    /// 
    ///     GET /api/GetAllManagerByEmployee
    /// </remarks>
    /// <response code="200">If the request is successfully processed and the manager associated with the employee is returned.</response>
    /// <response code="404">If no user is found for the specified email.</response>
    /// <response code="500">If an internal server error occurs.</response>        
    [HttpGet("GetAllManagerByEmployee")]
    public async Task<ManagerDto> GetAllManagerByEmployee(
        [FromQuery] GetManagerByEmployeeQuery query) =>
        await Mediator.Send(query);

    /// <summary>
    /// Retrieves the information of the logged-in employee.
    /// </summary>
    /// <returns>
    /// An object of type <see cref="EmployeeDto"/> representing the logged-in employee's information.
    /// </returns>
    /// <remarks>
    /// This method queries the database to fetch the employee information associated with the currently logged-in user,
    /// using their corporate email.
    /// 
    /// Example request:
    /// 
    ///     GET /api/GetLoggedEmployee
    /// </remarks>
    /// <response code="200">If the request is successfully processed and the employee information is returned.</response>
    /// <response code="404">If no employee is found for the specified email.</response>
    /// <response code="500">If an internal server error occurs.</response>        
    [HttpGet]
    public async Task<EmployeeDto> GetLoggedEmployee() => await Mediator.Send(new GetLoggedEmployeeQuery());

    /// <summary>
    /// Retrieves the signature associated with a corporate email.
    /// </summary>
    /// <param name="corporateEmail">
    /// The corporate email of the employee.
    /// </param>
    /// <param name="ct">Cancellation token for the asynchronous operation.</param>
    /// <returns>
    /// A string representing the employee's signature.
    /// </returns>
    /// <remarks>
    /// This method queries the database to fetch the signature associated with the provided corporate email.
    /// 
    /// Example request:
    /// 
    ///     GET /api/Signatures/{corporateEmail}
    /// </remarks>
    /// <response code="200">If the request is successfully processed and the employee's signature is returned.</response>
    /// <response code="404">If no signature is found for the specified corporate email.</response>
    /// <response code="500">If an internal server error occurs.</response>        
    [HttpGet("Signatures/{corporateEmail}")]
    public async Task<string> GetSignature(string corporateEmail, CancellationToken ct) =>
        await Mediator.Send(new GetSignatureQuery(corporateEmail), ct);
}
