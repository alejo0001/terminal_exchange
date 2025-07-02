using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Action = IntranetMigrator.Domain.Entities.Action;

namespace CrmAPI.Application.Processes.Commands.ReassignProcess;

[Authorize(Roles = "Supervisor")]
public class ReassignProcessCommand: IRequest
{
    public int ProcessId { get; set; }
    public int UserId { get; set; }
}

[UsedImplicitly]
public class ReassignProcessCommandHandler : IRequestHandler<ReassignProcessCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;
    private readonly IActionsService _actionService;
    private readonly ICurrentUserService _currentUserService;

    public ReassignProcessCommandHandler(
        IApplicationDbContext context,
        IDateTime dateTime,
        IActionsService actionService,
        ICurrentUserService currentUserService
    )
    {
        _context = context;
        _dateTime = dateTime;
        _actionService = actionService;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(ReassignProcessCommand request, CancellationToken ct)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u =>
                u.Employee.CorporateEmail == _currentUserService.Email, ct);

        var process = await _context.Processes
            .Where(p => !p.IsDeleted)
            .FirstOrDefaultAsync(p => p.Id == request.ProcessId, ct);

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            var fromUserId = process!.UserId ?? 0;
            process.UserId = request.UserId;

            var action = await SaveAction(request, request.UserId, process.ContactId, ct);

            var reassignment = await SaveReassignment(action.Id, fromUserId, request.UserId, user!.Id);

            await _context.SaveChangesAsync(ct);

            action.ReassignmentId = reassignment.Id;

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (Exception)
        {
            // ReSharper disable once MethodSupportsCancellation
#pragma warning disable CA2016
            await transaction.RollbackAsync();
#pragma warning restore CA2016

            throw;
        }

        return Unit.Value;
    }

    private async Task<Action> SaveAction(ReassignProcessCommand request, int userId, int contactId, CancellationToken cancellationToken)
    {
        var action = new Action 
        {
            ProcessId = request.ProcessId,
            ContactId = contactId,
            UserId = userId,    
            Type = ActionType.Reassign,
            Date = _dateTime.Now,
            Outcome = ActionOutcome.Reassigned,
        };
        return await _actionService.CreateAction(action, cancellationToken);

    }
        
    private Task<Reassignment> SaveReassignment(int actionId, int fromUserId, int  toUserId, int reassignerId)
    {
        Reassignment reassignment = new Reassignment() 
        {
            ActionId = actionId,
            FromUserId = fromUserId,
            ToUserId = toUserId,   
            ReassignerId = reassignerId,
        };
        _context.Reassignments.Add(reassignment);

        return Task.FromResult(reassignment);
    }
}