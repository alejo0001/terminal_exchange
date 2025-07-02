using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Action = IntranetMigrator.Domain.Entities.Action;

namespace CrmAPI.Application.Messages.Commands.SendMessageCommand;

public class SendMessageCommand : MessageSendDto, IRequest
{
}

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActionsService _actionsService;
    private readonly IProcessesService _processesService;
    private readonly IDateTime _dateTime;

    public SendMessageCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IActionsService actionsService,
        IProcessesService processesService,
        IDateTime dateTime)
    {
        _context = context;
        _currentUserService = currentUserService;
        _actionsService = actionsService;
        _processesService = processesService;
        _dateTime = dateTime;
    }

    public async Task<Unit> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Employee.CorporateEmail == _currentUserService.Email, cancellationToken);
        
        if (user == null)
        {
            throw new ForbiddenAccessException();
        }

        if (request.ContactLeadList is { Count: > 0 })
        {
            var leads = _context.ContactLeads.Where(lead => request.ContactLeadList.Contains(lead.Id));
            foreach (var lead in leads)
            {
                lead.SentMessage = true;
                _context.ContactLeads.Update(lead);
            }
        }

        var newAction = new Action
        {
            Guid = Guid.NewGuid(),
            ProcessId = request.ProcessId,
            ContactId = request.ContactId,
            UserId = user.Id,
            Type = ActionType.MessageSucceeded,
            Date = _dateTime.Now,
            Outcome = ActionOutcome.CommercialMessage,
        };

        await _actionsService.CreateAction(newAction, cancellationToken);
        await SetProcessOngoing(request, cancellationToken);

        var process = await _context.Processes
            .Where(p => p.Id == request.ProcessId && !p.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (process is not null)
        {
            process = SetColourToProcess(process, request.Colour);
            await _processesService.UpdateProcess(process, cancellationToken);    
        }

        await _processesService.SetIfIsFirstInteractionDateInProcess(newAction, cancellationToken);
        return Unit.Value;
    }

    private Process SetColourToProcess(Process process, Colour colour)
    {
        var processHasActiveCall = _context.Actions.Any(a =>
            a.ProcessId == process.Id && a.FinishDate == null && a.Type == ActionType.Call);

        if (!processHasActiveCall)
        {
            process.Colour = colour;
        }

        return process;
    }


    private async Task SetProcessOngoing(MessageSendDto request, CancellationToken cancellationToken)
    {
        var process = await _context.Processes
            .FirstOrDefaultAsync(p => p.Id == request.ProcessId, cancellationToken);

        if (process is not null)
        {
            process.Status = ProcessStatus.Ongoing;
            await _processesService.UpdateProcess(process, cancellationToken);
        }
    }
}