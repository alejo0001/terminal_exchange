using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CrmAPI.Application.Appointments.Queries.GetAppointmentsByProcess;

public class GetAppointmentsByProcessQuery : IRequest<AppointmentDto>
{
    public int ProcessId { get; set; }
}

public class GetAppointmentsByProcessQueryHandler : IRequestHandler<GetAppointmentsByProcessQuery, AppointmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IDateTime _dateTime;
    private readonly IWorkingDays _workingDaysService;
    private readonly IConfiguration _configuration;
    private readonly IProcessesService _processesService;

    public GetAppointmentsByProcessQueryHandler(IApplicationDbContext context, IMapper mapper, IConfiguration configuration,
        IDateTime dateTime, IWorkingDays workingDaysService, IProcessesService processesService)
    {
        _context = context;
        _mapper = mapper;
        _dateTime = dateTime;
        _workingDaysService = workingDaysService;
        _processesService = processesService;
        _configuration = configuration;
    }

    public async Task<AppointmentDto> Handle(GetAppointmentsByProcessQuery request, CancellationToken cancellationToken)
    {
        // SI EL PROCESO NO SE HA TOCADO, Y NO TIENE ACCIONES, DEVOLVEMOS LA ACCIÓN POR DEFECTO PARA ESTE TIPO DE PROCESO
        var process = await _context.Processes
            .Include(p => p.Contact)
            .Include(p => p.Actions)
            .Include(p => p.User.Employee)
            .FirstOrDefaultAsync(p => p.Id == request.ProcessId, cancellationToken);
        
        if (process!.Actions.Count == 0)
        {
            var processSetting = await _context.ProcessSettings
                .Where(pc => pc.ProcessType == process.Type || pc.ProcessType == null)
                .OrderByDescending(pc => pc.ProcessType)
                .FirstOrDefaultAsync(cancellationToken);

            var now = new AppointmentDto()
            {
                Title = processSetting!.FirstAppointmentType.ToString().ToLowerInvariant(),
                Date =  _dateTime.Now,
                Type = processSetting.FirstAppointmentType.ToString().ToLowerInvariant()
            };

            return now;
        }        
        
        // SI SE HA ESTABLECIDO UNA CITA Y ES FUTURA, DEVOLVEMOS LA CITA COMO PRÓXIMA INTERACCIÓN //
        var appointment = await _context.Appointments
            .Include(a => a.User)
            .Where(c => c.ProcessId == request.ProcessId && !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (appointment != null && appointment.Date > _dateTime.Now)
        {
            var dto = _mapper.Map<AppointmentDto>(appointment);
            dto.Type = appointment.Type.ToString().ToLowerInvariant();
            return dto;
        }


        // DEVOLVEMO LA PRÓXIMA INTERACCIÓN (QUE HA SIDO CALCULADA A LA HORA DE CREAR / ACTUALIZAR UNA ACCIÓN)
        return new AppointmentDto
        {
            Title = "call",
            Date = process.Contact.NextInteraction ?? DateTime.Now,
            Type = "call"
        };
        



/*


        // LE MANDAMOS LA PRÓXIMA INTEACCIÓN EN BASE A SI ESTA EN EL DÍA 0 O NO
        var dateNextInteraction =  _dateTime.Now.Date;

        // TODO: reviar este if, pues en unas líneas más arriba siempre le seteamos una valor a NextInteraction, nunca es null
        if (process.Contact.NextInteraction == null)
        {
            dateNextInteraction = dateNextInteraction.Date.Add(new TimeSpan(9, 00, 0));
        }
        else
        {
            dateNextInteraction = process.Contact.NextInteraction.Value;
        }


        var processDay = await _processesService.GetProcessDay(process, process.User.Employee);

        if (processDay == 0)
        {
            var userEmployee = await _context.Users
                .Include(e => e.Employee)
                .Where(u => u.Id == process.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            var absences = await _context.Users
                .Include(u => u.Employee)
                    .ThenInclude(e => e.Absences.Where(a => a.IsDeleted == false))
                .Where(u => u.Id == process.UserId)
                .Select(u => u.Employee.Absences)
                .FirstOrDefaultAsync(cancellationToken);

            var isFreeDay = true;
            var count = 1;
            while (isFreeDay && count < Int32.Parse(_configuration["Constants:MaxDaysFree"]))
            {
                dateNextInteraction = dateNextInteraction.AddDays(1);
                isFreeDay = _workingDaysService.IsFreeDay(userEmployee.Employee, absences, dateNextInteraction);
                count++;
            }
            dateNextInteraction = dateNextInteraction.Date.Add(new TimeSpan(9, 00, 0));
        }

        return new()
        {
            Title = "call",
            Date = dateNextInteraction,
            Type = "call"
        };*/
    }
}