using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;

namespace CrmAPI.Infrastructure.Services;

public class ActionsService: IActionsService
{

    private readonly IApplicationDbContext _context;

    public ActionsService( IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Action> CreateAction(Action action, CancellationToken cancellationToken)
    {
        _context.Actions.Add(action);
        await _context.SaveChangesAsync(cancellationToken);
        return action;
    }

    public async Task<Unit> DeleteAction(int actionId, CancellationToken cancellationToken)
    {
        var action = await _context.Actions.FindAsync(actionId);
        if (action == null)
        {
            return Unit.Value;
        }
        
        action.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
    
    public async Task<Action> UpdateAction(Action action, CancellationToken cancellationToken)
    {
        _context.Actions.Update(action);
        await _context.SaveChangesAsync(cancellationToken);
        return action;
    }

}