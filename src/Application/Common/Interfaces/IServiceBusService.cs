using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;

namespace CrmAPI.Application.Common.Interfaces;

public interface IServiceBusService
{
    Task AddEventToQueue(EventDto eventCalendar);
}