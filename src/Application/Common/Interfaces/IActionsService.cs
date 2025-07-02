using System.Threading;
using System.Threading.Tasks;
using IntranetMigrator.Domain.Entities;
using MediatR;

namespace CrmAPI.Application.Common.Interfaces;

public interface IActionsService
{
    Task<Action> CreateAction(Action action, CancellationToken cancellationToken);
    Task<Unit> DeleteAction(int actionId, CancellationToken cancellationToken);
    Task<Action> UpdateAction(Action action, CancellationToken cancellationToken);
}