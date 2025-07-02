using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Security;
using CroupierAPI.Contracts.Commands;
using CroupierAPI.Contracts.Dtos;
using CroupierAPI.Contracts.Events;
using MassTransit;
using MediatR;

namespace CrmAPI.Application.Coupons.Commands.CopyToCouponFpCommand;

[Authorize]
public record CopyToCouponFpCommand(int ContactId, int ProcessId)
    : CreateSpecialFpContactCouponDto(ContactId, ProcessId),
        IRequest<bool>;

[UsedImplicitly]
public class CopyToCouponFpCommandHandler : IRequestHandler<CopyToCouponFpCommand, bool>
{
    private readonly IRequestClient<CreateSpecialFpContactCoupon> _requestClient;

    public CopyToCouponFpCommandHandler(IRequestClient<CreateSpecialFpContactCoupon> requestClient) =>
        _requestClient = requestClient;

    /// <inheritdoc />
    public async Task<bool> Handle(CopyToCouponFpCommand request, CancellationToken ct)
    {
        var command = new CreateSpecialFpContactCoupon(request.ContactId, request.ProcessId);

        var response = await _requestClient.GetResponse<SpecialFpContactCouponCreated>(command, ct);

        return response.Message.IsSuccess;
    }
}
