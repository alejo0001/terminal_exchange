using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using MediatR;

namespace CrmAPI.Application.OrdersImported.Commands.DeleteOrderImportedFromTlmkByEmail;

public class DeleteOrderImportedFromTlmkByEmailCommand: IRequest<List<int>>
{
    public List<string> Emails { get; set; }
}

public class DeleteOrderImportedFromTlmkByEmailCommandHandler : IRequestHandler<DeleteOrderImportedFromTlmkByEmailCommand, List<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrderImportedService _orderImportedService;

    public DeleteOrderImportedFromTlmkByEmailCommandHandler(IApplicationDbContext context, IOrderImportedService orderImportedService)
    {
        _context = context;
        _orderImportedService = orderImportedService;
    }

    public async Task<List<int>> Handle(DeleteOrderImportedFromTlmkByEmailCommand request, CancellationToken cancellationToken)
    {
        return await _orderImportedService.DeleteOrderImportedFromTlmkByEmail(request.Emails, cancellationToken);
    }
}