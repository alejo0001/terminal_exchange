using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using MediatR;

namespace CrmAPI.Application.Emails.Queries.GetMailBoxFree;

public class GetMailBoxFreeQuery : IRequest<bool>
{
}

public class GetMailBoxFreeQueryHandler : IRequestHandler<GetMailBoxFreeQuery, bool>
{
        
    private readonly ICurrentUserService _currentUserService;
        
        
    public GetMailBoxFreeQueryHandler(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(GetMailBoxFreeQuery request, CancellationToken cancellationToken)
    {
        // TODO: AQUI FALTA CÓDIGO
            
        return false;
    }
}