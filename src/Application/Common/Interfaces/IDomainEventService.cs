using System.Threading.Tasks;
using CrmAPI.Domain.Common;

namespace CrmAPI.Application.Common.Interfaces;

public interface IDomainEventService
{
    Task Publish(DomainEvent domainEvent);
}