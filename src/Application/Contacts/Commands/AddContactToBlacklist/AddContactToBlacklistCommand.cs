using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Action = IntranetMigrator.Domain.Entities.Action;
using Contact = IntranetMigrator.Domain.Entities.Contact;

namespace CrmAPI.Application.Contacts.Commands.AddContactToBlacklist;

public class AddContactToBlacklistCommand : IRequest<int>
{
    public int ContactId { get; set; }
    public int NewContactStatusId { get; set; }
    public string? NewStatusReason { get; set; }
    public string? NewStatusObservations { get; set; }
    public int ProcessId { get; set; }
}

public class AddContactToBlacklistCommandHandler : IRequestHandler<AddContactToBlacklistCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ILeadsDbContext _leadsDbContext;
    private readonly IProcessesService _processesService;
    private readonly ILogger<AddContactToBlacklistCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;
    private readonly ICalendarService _calendar;
    private readonly IPotentialsService _potentialsService;

    public AddContactToBlacklistCommandHandler(IApplicationDbContext context, ILeadsDbContext leadsDbContext,
        ILogger<AddContactToBlacklistCommandHandler> logger, ICurrentUserService currentUserService,
        IDateTime dateTime, ICalendarService calendar, IPotentialsService potentialsService, IProcessesService processesService)
    {
        _context = context;
        _leadsDbContext = leadsDbContext;
        _logger = logger;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _calendar = calendar;
        _potentialsService = potentialsService;
        _processesService = processesService;
    }

    public async Task<int> Handle(AddContactToBlacklistCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Employee.CorporateEmail == _currentUserService.Email, cancellationToken);
        if (user == null)
        {
            throw new ForbiddenAccessException();
        }


        var contact = await _context.Contact
            .Include(c => c.Appointments.Where(a => !a.IsDeleted))
            .Where(c => c.Id == request.ContactId)
            .FirstOrDefaultAsync(cancellationToken);

        if (contact == null)
            throw new NotFoundException(nameof(Contact), request.ContactId);

        var process = await _context.Processes
            .Where(c => c.Id == request.ProcessId)
            .FirstOrDefaultAsync(cancellationToken);

        if (process == null)
            throw new NotFoundException();

        var contactNotValidStatus = await _context.ContactStatus
            .FirstOrDefaultAsync(c => c.Name == "NotValid", cancellationToken);

        if (!String.IsNullOrWhiteSpace(request.NewStatusObservations))
        {
            request.NewStatusReason = $"{request.NewStatusReason}: {request.NewStatusObservations}";
        }

        contact.ContactStatusId = request.NewContactStatusId;
        contact.Observations = request.NewStatusReason;
        contact.NextInteraction = null;
        process.Outcome = request.NewContactStatusId == contactNotValidStatus.Id ? ProcessOutcome.NotValid : ProcessOutcome.NotSale;
        process.Status = ProcessStatus.Closed;

        await _calendar.DeleteAllContactEvents(contact, cancellationToken).ConfigureAwait(false);

        CreateNewAction(request, user, process);
        await _processesService.ClearDiscountContactLeads(process.ContactId, cancellationToken);

        var result = await _context.SaveChangesAsync(cancellationToken);
        // Update contact status in leads db
        if (contact.OriginContactId is not null)
        {
            await _potentialsService.UpdateStatusContactInPotentials(contact.OriginContactId.Value, request.NewContactStatusId, cancellationToken);
        }
        _logger.LogInformation($"Result of the query {result}");
        return result;
    }



    private void CreateNewAction(AddContactToBlacklistCommand request, User user, Process process)
    {
        Action newAction = new Action
        {
            UserId = user.Id,
            ContactId = request.ContactId,
            User = user,
            Date = _dateTime.Now,
            FinishDate = _dateTime.Now,
            ProcessId = process.Id,
            Type = ActionType.BlackList,
            Outcome = ActionOutcome.Blacklist
        };

        _context.Actions.Add(newAction);
    }
}
