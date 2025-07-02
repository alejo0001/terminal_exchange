using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Action = IntranetMigrator.Domain.Entities.Action;

namespace CrmAPI.Application.Processes.Commands.ReassignAllUserProcesses;

public class ReassignAllUserProcessesCommand: IRequest
{
    public int FromUserId { get; set; }
    public int ToUserId { get; set; }
    public bool OnlyToDo { get; set; } = false;
}

[UsedImplicitly]
public class ReassignProcessCommandHandler : IRequestHandler<ReassignAllUserProcessesCommand>
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

    public async Task<Unit> Handle(ReassignAllUserProcessesCommand request, CancellationToken ct)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u =>
                u.Employee.CorporateEmail == _currentUserService.Email, ct);

        var queryProcesses = _context.Processes
            .Where(p => p.UserId == request.FromUserId)
            .Where(p => !p.IsDeleted)
            .Where(p => p.Status != ProcessStatus.Closed).AsQueryable();

        if (request.OnlyToDo)
        {
            queryProcesses = queryProcesses.Where(p => p.Status == ProcessStatus.ToDo && p.Colour == Colour.Grey);
        }

        var processes = await queryProcesses.ToListAsync(ct);

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            foreach (var process in processes)
            {
                process.UserId = request.ToUserId;

                var action = await SaveAction(process.Id, request.ToUserId, process.ContactId, ct);

                Reassignment reassignment = await SaveReassignment(action.Id, request.FromUserId, request.ToUserId, user!.Id);
                _context.Reassignments.Add(reassignment);
                await _context.SaveChangesAsync(ct);

                action.ReassignmentId = reassignment.Id;
                await _context.SaveChangesAsync(ct);
            }

            await transaction.CommitAsync(ct);
        }
        catch (Exception)
        {
#pragma warning disable CA2016
            // ReSharper disable once MethodSupportsCancellation
            await transaction.RollbackAsync();
#pragma warning restore CA2016

            throw;
        }
        
        return Unit.Value;
    }
        
    private async Task<Action> SaveAction(int processId, int userId, int contactId, CancellationToken cancellationToken)
    {
        var action = new Action 
        {
            ProcessId = processId,
            ContactId = contactId,
            UserId = userId,    
            Type = ActionType.Reassign,
            Date = _dateTime.Now,
            Outcome = ActionOutcome.Reassigned,
        };
        return await _actionService.CreateAction(action, cancellationToken);

    }
        
    private Task<Reassignment> SaveReassignment(int actionId, int fromUserId, int toUserId, int reassignerId)
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