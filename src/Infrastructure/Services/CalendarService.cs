using System;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CrmAPI.Infrastructure.Services;

public class CalendarService : ICalendarService
{
    private readonly ICalendarApiClient _apiClient;
    private readonly ILogger<CalendarService> _logger;
    private readonly ICurrentUserService _currentUserService;

    public CalendarService(
        ICalendarApiClient apiClient,
        ILogger<CalendarService> logger,
        ICurrentUserService currentUserService)
    {
        _apiClient = apiClient;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<string> CreateEvent(Appointment appointment, CancellationToken ct)
    {
        try
        {
            var eventCalendar = new EventDto
            {
                UserTag = await _currentUserService.GetUserOid(),
                Subject = CreateEventSubject(appointment),
                StartDate = appointment.Date,
                EndDate = appointment.Date,
                AllowNewTimeProposals = true,
            };

            var response = await _apiClient.CreateEvent(eventCalendar, ct).ConfigureAwait(false);

            // TODO: I left it as is (time pressure), because code before changes was a bit sluggish already.
            var result = await response.Content
                .ReadFromJsonAsync<EventCalendarModelDto>(options: null, ct).ConfigureAwait(false);

            _logger.LogInformation("Response of the CalendarAPI.CreateEvent: {@Response}", result);

            return result?.Id ?? string.Empty;
        }
        catch (Exception e)
        {
            _logger.LogInformation(e.Message);
            return "";
        }
    }

    public async Task<string> UpdateEvent(Appointment appointment, CancellationToken ct)
    {
        var eventCalendar = new EventDto
        {
            UserTag = await _currentUserService.GetUserOid(),
            Subject = CreateEventSubject(appointment),
            EventId = appointment.EventId,
            StartDate = appointment.Date,
            EndDate = appointment.Date,
            AllowNewTimeProposals = true,
        };

        var response = await _apiClient.UpdateEvent(eventCalendar, ct).ConfigureAwait(false);

        // TODO: I left it as is (time pressure), because code before changes was a bit sluggish already.
        var result = await response.Content
            .ReadFromJsonAsync<EventCalendarModelDto>(options: null, ct).ConfigureAwait(false);

        _logger.LogInformation("Response of the CalendarAPI.CreateEvent: {@Response}", result);

        return result?.Id ?? string.Empty;
    }

    public async Task<string> DeleteEvent(Appointment appointment, CancellationToken ct)
    {
        var response = await _apiClient.DeleteEvent(appointment.EventId, ct).ConfigureAwait(false);

        var result = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        _logger.LogInformation("Response of the CalendarAPI.CreateEvent: {Response}", result);

        return result;
    }

    public async Task DeleteAllContactEvents(Contact contact, CancellationToken ct)
    {
        foreach (var appointment in contact.Appointments)
        {
            if (appointment.EventId != null)
            {
                await DeleteEvent(appointment, ct).ConfigureAwait(false);
                appointment.EventId = null;
            }

            appointment.IsDeleted = true;
        }
    }

    private static string CreateEventSubject(Appointment appointment) =>
        $"{GetSubjectAction(appointment.Type)} {appointment.Contact.Name} {appointment.Contact.FirstSurName}";

    private static string GetSubjectAction(AppointmentType appointmentType) =>
        //TODO: aplicar traducciones a los eventos del calendario
        appointmentType switch
        {
            //TODO: aplicar traducciones a los eventos del calendario
            AppointmentType.Call => "Llamar a",
            AppointmentType.Email => "Enviar correo a",
            AppointmentType.WhatsApp => "Enviar whatsapp a",
            _ => string.Empty,
        };
}
