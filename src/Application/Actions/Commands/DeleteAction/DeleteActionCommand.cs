using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using MediatR;

namespace CrmAPI.Application.Actions.Commands.DeleteAction;

public class DeleteActionCommand : IRequest
{
    public int Id { get; set; }
}
    
public class DeleteActionCommandHandler : IRequestHandler<DeleteActionCommand>
{
    private readonly IActionsService _actionService;

    public DeleteActionCommandHandler(IActionsService actionService)
    {
        _actionService = actionService;
    }
        
    public async Task<Unit> Handle(DeleteActionCommand request, CancellationToken cancellationToken)
    {
        
        return await _actionService.DeleteAction(request.Id, cancellationToken);
    }
}