using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using Domain.Tlmk.Entities;

namespace CrmAPI.Application.Common.Interfaces;

public interface IOrderImportedService
{
    Task<List<OrdersImportedDto>> GetOrdersImportedByContactId(int contactId, CancellationToken cancellationToken);
    Task<List<PedidoTlmk>> GetPedidoByContactEmail(string email, CancellationToken cancellationToken);
    Task<List<PedidoTlmk>> GetPedidoByContactEmails(List<string> emails, CancellationToken cancellationToken);
    Task<List<PedidoTlmk>> GetPedidoByContactDni(string dni, CancellationToken cancellationToken);
    OrdersImportedDto GetOrderImportedDtoFromPedidoTlmk(PedidoTlmk pedidoTlmk, int contactId);
    List<OrdersImportedDto> GetOrdersImportedDtoListFromPedidoTlmkList(List<PedidoTlmk> pedidosTlmk, int contactId);
    Task<List<int>> DeleteOrderImportedFromTlmkByEmail(List<string> emails, CancellationToken cancellationToken);
}
