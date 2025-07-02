using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Coupons.Commands.CopyToCouponFpCommand;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class CouponsController : ApiControllerBase
{
    /// <summary>
    /// Copies the information of a contact to a FP coupon.
    /// </summary>
    /// <param name="command">
    /// Command that contains the necessary parameters for the operation:
    /// - <see cref="CopyToCouponFpCommand.ContactId"/>: Identifier of the contact.
    /// - <see cref="CopyToCouponFpCommand.ProcessId"/>: Identifier of the process.
    /// </param>
    /// <param name="ct">Cancellation token for the asynchronous operation.</param>
    /// <returns>
    /// A boolean value indicating whether the operation was successful.
    /// </returns>
    /// <remarks>
    /// This method sends a command to copy the contact information to a FP coupon.
    /// 
    /// Example request:
    /// 
    ///     POST /api/CopyToCouponFp
    ///     {
    ///         "ContactId": 123,
    ///         "ProcessId": 456
    ///     }
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the operation is completed.</response>
    /// <response code="400">If the request is invalid or parameters are missing.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("CopyToCouponFp")]
    public async Task<bool> CopyToCouponFp(CopyToCouponFpCommand command, CancellationToken ct) =>
        await Mediator.Send(command, ct);
}
