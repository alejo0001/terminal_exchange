using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationAPI.Contracts.Commands;
using Action = IntranetMigrator.Domain.Entities.Action;
using DateTime = System.DateTime;

namespace CrmAPI.Infrastructure.Services;

public class ProcessesService : IProcessesService
{
    private readonly IDateTime _dateTime;
    private readonly IApplicationDbContext _context;
    private readonly ILeadsDbContext _leadsDbContext;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly ICalendarService _calendar;
    private readonly IActionsService _actionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITranslationsService _translationsService;
    private readonly IOrganizationNodeExplorerService _organizationNodeExplorerService;
    private readonly IWorkingDays _workingDaysService;
    private readonly IWorkScheduleService _workScheduleService;
    private readonly IBus _bus;
    private readonly ILogger<ProcessesService> _logger;
    private readonly IPotentialsService _potentialsService;
    IRecoverContactActivationsCommand _recoverService;

    public ProcessesService(
        IDateTime dateTime,
        IApplicationDbContext context,
        ILeadsDbContext leadsDbContext,
        IConfiguration configuration,
        ICalendarService calendar,
        IActionsService actionService,
        ICurrentUserService currentUserService,
        ITranslationsService translationsService,
        IWorkingDays workingDaysService,
        IOrganizationNodeExplorerService organizationNodeExplorerService,
        IBus bus,
        IWorkScheduleService workScheduleService,
        IMapper mapper,
        ILogger<ProcessesService> logger,
        IPotentialsService potentialsService,
        IRecoverContactActivationsCommand recoverService)
    {
        _dateTime = dateTime;
        _context = context;
        _leadsDbContext = leadsDbContext;
        _configuration = configuration;
        _calendar = calendar;
        _actionService = actionService;
        _currentUserService = currentUserService;
        _translationsService = translationsService;
        _workingDaysService = workingDaysService;
        _organizationNodeExplorerService = organizationNodeExplorerService;
        _mapper = mapper;
        _logger = logger;
        _bus = bus;
        _workScheduleService = workScheduleService;
        _potentialsService = potentialsService;
        _recoverService = recoverService;
    }

    public async Task<int> GetProcessDay(Process process)
    {
        var processSetting = _context.ProcessSettings
            .FirstOrDefault(ps => ps.ProcessType == process.Type);

        int currentDay = processSetting?.StartingFromDay ?? 1;

        var processTypesStartingNotDayZero = new[]
        {
            ProcessType.Visits,
            ProcessType.Coupons,
            ProcessType.CouponsContact,
            ProcessType.CouponsDiscount,
            ProcessType.CouponsDossier,
            ProcessType.CouponsInformation,
            ProcessType.CouponsRematricula,
            ProcessType.CouponsPrePurchase
        };

        var actionTypes = new[]
        {
            ActionType.Call,
            ActionType.EmailSucceeded,
            ActionType.MessageSucceeded
        };

        var processType = processSetting?.ProcessType ?? ProcessType.CouponsContact;

        List<Action> actions;

        //Esto es para controlar que los cupones no pasen de día ya que se controla diferente.
        //Ahora tenemos en cuenta solo la llamada necesaria y no los emails automáticos
        var generalActions = _context.Actions
            .Where(a => a.Date.Date < _dateTime.Now.Date)
            .Where(a => actionTypes.Contains(a.Type))
            .Where(a => a.ProcessId == process.Id);

        if (processTypesStartingNotDayZero.Contains(processType))
        {
            actions = await generalActions
                .Where(a => a.CreatedBy != "System")
                .ToListAsync();
        }
        else
        {
            actions = await generalActions
                .ToListAsync();
        }

        if (actions.Count == 0)
        {
            return currentDay;
        }

        var actionsDictionary = actions.DistinctBy(a => a.Date.Date)
            .ToDictionary(a => a.Date.Date, _ => 1);

        currentDay = processSetting?.StartingFromDay == 0 ? actionsDictionary.Count : actionsDictionary.Count + 1;

        return currentDay;
    }

    public async Task<int> GetProposalProcessDay(Process process)
    {
        int currentDay = await GetProcessDay(process);

        var processSetting = await _context.ProcessSettings
            .Where(pc => pc.ProcessType == process.Type || pc.ProcessType == null)
            .OrderByDescending(pc => pc.ProcessType)
            .FirstOrDefaultAsync();

        currentDay = currentDay < processSetting.StartingFromDay ? processSetting.StartingFromDay :
            currentDay > processSetting.MaxDays ? processSetting.MaxDays : currentDay;

        return currentDay;
    }

    public async Task<int> ComputeAttempts(List<Action> actions, ProcessType processType)
    {
        if (actions == null)
        {
            return 0;
        }

        var processSetting = await _context.ProcessSettings
            .Where(pc => pc.ProcessType == processType || pc.ProcessType == null)
            .OrderByDescending(pc => pc.ProcessType)
            .FirstOrDefaultAsync();

        var attempt = actions.Count(a => a.Type == ActionType.Call
                                         && a.Outcome != ActionOutcome.WrongCall
                                         && a.Date.Date == _dateTime.Now.Date);

        attempt = attempt > processSetting!.MaxAttempts ? processSetting.MaxAttempts : attempt;

        return attempt;
    }

    public async Task<Process> SetColourProcess(Process process, Action action, CancellationToken ct)
    {
        // Si la accion es de tipo llamada y el outome es comercialEmail, quiere decir que desde el front
        // el contacto contestó y que se le envió información comercial. Pasamos el proceso a verde
        if (action.Type == ActionType.Call &&
            (action.Outcome is ActionOutcome.CommercialEmail or ActionOutcome.CommercialMessage) &&
            process.Colour is Colour.Yellow or Colour.Grey
           )
        {
            process.Colour = Colour.Green;
            // process = await SetInitialDate(process, cancellationToken);
        }

        // Si la accion es de tipo llamada y el outome es noResponse, quiere decir que desde el front
        // el contacto no contestó, pero se le envió información comercial. Pasamos el proceso a amarillo
        if (action.Type == ActionType.Call &&
            action.Outcome == ActionOutcome.NoResponse && process.Colour is Colour.Yellow or Colour.Grey
           )
        {
            process.Colour = Colour.Yellow;
        }

        // Si la accion es de tipo llamada y el outome es noResponse, quiere decir que desde el front
        // el contacto no contestó, pero ya se habia contactado y enviado correo comercial,
        // se le envió correo recordatorio de cita / Beca / plaza. Pasamos el proceso a verde-amarillo
        if (action.Type == ActionType.Call &&
            action.Outcome == ActionOutcome.NoResponse && process.Colour == Colour.Green
           )
        {
            process.Colour = Colour.GreenYellow;
        }

        // Si la accion es de tipo llamada y el outome es comercialEmail y el contacto es verde amarillo
        // significa q se le envía otro correo comercial, con lo que vuelve a pasar a verde
        if (action.Type == ActionType.Call &&
            (action.Outcome is ActionOutcome.CommercialEmail or ActionOutcome.CommercialMessage) &&
            process.Colour == Colour.GreenYellow
           )
        {
            process.Colour = Colour.Green;
            // process = await SetInitialDate(process, cancellationToken);
        }

        // En cualquier caso, si el proceso es de tipo impago, se tiene que mantener rojo
        if (process.Type == ProcessType.NonPayment)
        {
            process.Colour = Colour.Red;
        }

        return process;
    }



    public async Task<int> GetTotalTriesCallByDay(ProcessType processType, CancellationToken ct)
    {
        var defaultValue = Int32.Parse(_configuration["Constants:ActionCallDailyLimitPerProcess"]);

        var processSetting = await _context.ProcessSettings
            .Where(ps => ps.ProcessType == processType)
            .FirstOrDefaultAsync(ct);

        if (processSetting is not null)
        {
            defaultValue = processSetting.MaxAttempts;
        }

        return defaultValue;
    }

    public async Task ClearDiscountContactLeads(int contactId, CancellationToken ct)
    {
        var contactLeads = await _context.ContactLeads
            .Where(cl => !cl.IsDeleted && cl.ContactId == contactId)
            .ToListAsync(ct);

        contactLeads.ForEach(contactLead =>
        {
            contactLead.Discount = null;
            contactLead.FinalPrice = null;
        });

        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> CheckIfProcessIsReplaceable(Process process, CancellationToken ct)
    {
        if (process.Colour == Colour.Green)
        {
            return false;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        // Cerrar el proceso y lo pasamos al dia 1. CRM-3448
        if (process.Id != 0)
        {          
            await _recoverService.UpdateProcess(process.Id, ct);
            await _recoverService.UpdateAction(process.Id, ct);

            var changes = await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            if (changes == 0)
            {
                _logger.LogWarning("No se realizaron cambios en la base de datos.");
            }
        }

        var user = await _context.Users
            .Include(u => u.Employee)
                .ThenInclude(e => e.CurrentOrganizationNode)
            .FirstOrDefaultAsync(u =>
                u.Employee.CorporateEmail == _currentUserService.Email, ct);

        var tagId = await _organizationNodeExplorerService
            .GetTagIdNode(user!.Employee.CurrentOrganizationNode, _configuration["Constants:TagCrmPriorityReplace"]!);

        if (tagId == 0)
        {
            return false;
        }

        var actualProcessOwnerTagId = await _organizationNodeExplorerService
            .GetTagIdNode(process.User.Employee.CurrentOrganizationNode,
                _configuration["Constants:TagCrmPriorityReplace"]!);

        return actualProcessOwnerTagId == 0;
    }

    public async Task OnCloseProcessActions(int processId, Process? process, Action? action,
        DiscardReasonProcess? discardReason,
        CancellationToken ct)
    {
        if (process is null)
        {
            process = await _context.Processes.FindAsync(processId);
        }

        if (process is null)
        {
            return;
        }

        var contact = await _context.Contact
            .Include(c => c.Appointments.Where(a => !a.IsDeleted))
            .Where(c => c.Id == process.ContactId)
            .FirstOrDefaultAsync(ct);

        if (contact is null)
        {
            return;
        }

        // LIMPIAMOS TODAS LAS CITAS DEL USUARIO PARA ESTE PROCESO
        await _calendar.DeleteAllContactEvents(contact, ct).ConfigureAwait(false);
        contact.NextInteraction = null;
        await _context.SaveChangesAsync(ct);

        // CREAMOS LA ACCIÓN 
        await _actionService.CreateAction(action, ct);

        // CREAMOS EL MOTIVO DEL DESCARTE SI LO HUBIERA
        if (discardReason is not null)
        {
            await AddDiscardReasonProcess(discardReason, ct);
        }

        // LIMPIAMOS LOS DESCUENTOS QUE SE LE HAN HECHO AL CONTACTO A LOS CURSOS INTERESADOS
        await ClearDiscountContactLeads(process.ContactId, ct);

    }

    public async Task<int> CreateProcess(Process process, CancellationToken ct)
    {
        _context.Processes.Add(process);

        process.Type = await GetProcessTypeFromEmployee(process, ct);

        await ResetNextInteractionContactInCreateProcess(process.ContactId, ct);
        await ResetCoursePricesForProcess(new List<Process> { process }, ct);
        await _context.SaveChangesAsync(ct);
        return process.Id;
    }

    public async Task<int> CreateProcess(ProcessCreateDto processDto, CancellationToken ct)
    {
        var process = _mapper.Map<Process>(processDto);

        process.Type = await GetProcessTypeFromEmployee(process, ct);

        _context.Processes.Add(process);
        await ResetNextInteractionContactInCreateProcess(process.ContactId, ct);
        await _context.SaveChangesAsync(ct);
        await ResetCoursePricesForProcess(new List<Process> { process }, ct);
        return process.Id;
    }

    private async Task<ProcessType> GetProcessTypeFromEmployee(Process process, CancellationToken ct)
    {
        var processTypeMap = new Dictionary<string, ProcessType>(StringComparer.OrdinalIgnoreCase)
        {
            { "Coupons", ProcessType.CouponsContact },
            { "Visits", ProcessType.Visits },
            { "Activations", ProcessType.Activations },
            { "Records2", ProcessType.Records2 }
        };

        var employeeContactType = await _context.Employees
            .Include(e => e.User)
            .Where(e => e.User.Id == process.UserId &&
                        !e.IsDeleted &&
                        !e.User.IsDeleted)
            .Select(e => e.ContactType)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrEmpty(employeeContactType))
        {
            _logger.LogError("Employee or ContactType is null or Empty in {ProcessId}", process.Id);
            employeeContactType = ProcessType.Records2.ToString();
        }

        if (!processTypeMap.TryGetValue(employeeContactType, out var typeProcess))
        {
            _logger.LogError("No matches found on the processTypeMap. ProcessId:{ProcessId}", process.Id);
        }

        return typeProcess;
    }

    private async Task ResetNextInteractionContactInCreateProcess(int contactId, CancellationToken ct)
    {
        var contact = await _context.Contact.FirstOrDefaultAsync(c => c.Id == contactId, ct);
        if (contact is not null)
        {
            contact.NextInteraction = null;
            await SetLeadLastDealDate(contact, ct);
        }
    }

    private async Task SetLeadLastDealDate(Contact contact, CancellationToken ct)
    {
        if (contact.OriginContactId is not null)
        {
            var lead = await _leadsDbContext.Leads
                .FirstOrDefaultAsync(l => l.id == contact.OriginContactId, ct);
            if (lead != null)
            {
                lead.fecha_reparto = _dateTime.Now;
                await _leadsDbContext.SaveChangesAsync(ct);
            }
        }
    }


    public async Task AddDiscardReasonProcess(DiscardReasonProcess? discardReasonProcess, CancellationToken ct)
    {
        if (discardReasonProcess is null)
        {
            return;
        }

        // NOS ASEGURAMOS DEL QUE EL ID DE LA RAZÓN EXISTA
        var reason = await _context.DiscardReasons
            .Where(dr => dr.Id == discardReasonProcess.DiscardReasonId)
            .FirstOrDefaultAsync(ct);

        if (reason == null)
        {
            return;
        }

        _context.DiscardReasonProcesses.Add(discardReasonProcess);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Process> UpdateProcessFromProcess(Process processNewFields, CancellationToken ct)
    {
        var process = await _context.Processes
            .Include(p => p.Contact.ContactEmail)
            .Include(p => p.Contact.ContactLanguages)
            .Where(p => p.Id == processNewFields.Id)
            .FirstOrDefaultAsync(ct).ConfigureAwait(false);

        if (process is null)
        {
            return processNewFields;
        }

        PropertyInfo[] properties = process.GetType().GetProperties();
        foreach (PropertyInfo property in properties)
        {
            var newValue = property.GetValue(processNewFields, null);
            if (newValue != null && !property.Name.Equals("Id") &&
                !property.Name.Contains("Created")
                && !property.Name.Equals("Guid"))
            {
                process.GetType().GetProperty(property.Name)?.SetValue(process, newValue, null);
            }
        }

        return await UpdateProcess(process, ct);
    }

    public async Task<Process> UpdateProcess(Process process, CancellationToken ct)
    {
        _context.Processes.Update(process);
        await _context.SaveChangesAsync(ct);
        return process;
    }

    public async Task ResetCoursePricesForProcess(List<Process> processes, CancellationToken ct)
    {
        var contactIds = processes.Select(p => p.ContactId).ToList();

        var leads = await _context.ContactLeads
            .Where(l => contactIds.Contains(l.ContactId ?? 0) && !l.IsDeleted)
            .ToListAsync(ct);

        foreach (var lead in leads)
        {
            lead.Discount = 0;
            lead.FinalPrice = lead.Price;
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<DateTime?> GetNextInteractionDateWhenThereIsNoResponse(Process process, DateTime date,
        CancellationToken ct)
    {
        // SI NO TIENE CITA, CALCULAMOS CUANDO LE VAMOS A VOLVER A LLAMAR. SI HOY NO HAN
        // HABIDO MÁS DEL INTENTO MÁXIMO POR DIA, LE SUMAMOS 3 HORAS A LA FECHA DE LA ULTIMA LLAMADA. SI LO HA SUPERADO
        // BUSCAMOS UNA FECHA VÁLIDA DE PRÓXIMA INTERACCIÓN Y LA SETEMAMOS A LAS 9:00 (POR AHORA)
        var actions = await GetTotalCallsTodayInProcess(process.Id, process.ContactId, ct);

        // SI YA HEMOS HECHO EL TOTAL DE LLAMADAS DIARIAS, PROPONEMOS LA PRÓXIMA INTERACCIÓN AL PRÓXIMO DÍA VÁLIDO
        if (actions.Count >= await GetTotalTriesCallByDay(process.Type, ct))
        {
            return await GetNextInteractionValid(process, ct);
        }

        // (ES) Siempre que exista un proceso, existe un ID asociado.
        // ------------------------------------------------------------------------------------
        // (EN) Whenever a process exists, there must be an associated ID
        var id = process.UserId.GetValueOrDefault();

        //return actions[0].Date.AddMinutes(Int32.Parse(_configuration["Constants:MinutesNextInteraction"]));
        return await _workScheduleService.GetProposalNextDate(id, date, ct);
    }

    public async Task<List<Action>> GetTotalCallsTodayInProcess(int processId, int contactId,
        CancellationToken cancellationToken)
    {
        return await _context.Actions
            .Where(a => a.ContactId == contactId
                        && a.ProcessId == processId
                        && a.Type == ActionType.Call
                        && a.Outcome != ActionOutcome.WrongCall
                        && a.Date.Date == _dateTime.Now.Date)
            .OrderByDescending(c => c.Created)
            .ToListAsync(cancellationToken);
    }

    private async Task<DateTime> GetNextInteractionValid(Process process, CancellationToken ct)
    {
        var userEmployee = await _context.Users
            .Include(e => e.Employee)
            .Where(u => u.Id == process.UserId)
            .FirstOrDefaultAsync(ct);

        var nextInteractionDate = await _workingDaysService.GetNextLaboralDay(userEmployee!.Employee, ct);

        return new DateTime(
            nextInteractionDate.Year,
            nextInteractionDate.Month,
            nextInteractionDate.Day,
            8,
            0,
            0
        );
    }

    public async Task SetIfIsFirstInteractionDateInProcess(Action action, CancellationToken ct)
    {
        if (action.Type == ActionType.EmailSucceeded
            || action.Type == ActionType.MessageSucceeded)
        {
            var process = await _context.Processes.FindAsync(action.ProcessId);

            if (process is null)
            {
                return;
            }

            if (process.FirstInteractionDate is null)
            {
                process.FirstInteractionDate = action.Date;
            }

            await UpdateProcess(process, ct);
        }
    }

    /// <summary>
    /// Obtener los IDs de contactos asociados a los procesos de un usuario
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<List<int>> GetContactOriginContactIdUserIdAsync(int userId, CancellationToken ct)
    {
        return await _context.Processes
            .AsNoTracking()
            .Include(p => p.Contact)
            .Where(p => p.UserId == userId && p.Status != ProcessStatus.Closed && (p.Type == ProcessType.Records2 || p.Type == ProcessType.Activations))
            .Where(x => !x.IsDeleted)
            .Select(p => p.Contact.OriginContactId)
            .Where(id => id.HasValue)
            .Select(id => id.Value)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Obtener el ID del usuario a partir del correo electrónico corporativo
    /// </summary>
    /// <param name="corporateEmail"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<int> GetUserIdCorporateEmail(string corporateEmail, CancellationToken ct)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Employee.CorporateEmail == corporateEmail && !u.IsDeleted)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(ct);
    }
}