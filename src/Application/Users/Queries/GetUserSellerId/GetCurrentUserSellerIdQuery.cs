using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Users.Queries.GetUserSellerId;

public class GetCurrentUserSellerIdQuery : IRequest<int?>
{
}

public class GetCurrentUserSellerIdQueryHandler : IRequestHandler<GetCurrentUserSellerIdQuery, int?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentUserSellerIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }
        
    public async Task<int?> Handle(GetCurrentUserSellerIdQuery request, CancellationToken cancellationToken)
    {
        // 
        return await _context.Employees
            .Where(e => e.CorporateEmail == _currentUserService.Email)
            .Select(e => e.IdVendedor)
            .FirstOrDefaultAsync(cancellationToken);
    }
}