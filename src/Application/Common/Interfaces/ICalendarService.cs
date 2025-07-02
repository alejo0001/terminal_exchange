using System.Threading;
using System.Threading.Tasks;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Interfaces;

public interface ICalendarService
{
    Task<string> CreateEvent(Appointment appointment, CancellationToken ct);

    Task<string> UpdateEvent(Appointment appointment, CancellationToken ct);

    Task<string> DeleteEvent(Appointment appointment, CancellationToken ct);

    Task DeleteAllContactEvents(Contact contact, CancellationToken ct);
}
