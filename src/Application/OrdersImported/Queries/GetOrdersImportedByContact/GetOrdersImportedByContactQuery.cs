using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using Domain.Tlmk.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;


namespace CrmAPI.Application.OrdersImported.Queries.GetOrdersImportedByContact;

[Authorize(Roles = "Usuario")]
public class GetOrdersImportedByContactQuery: IRequest<List<OrdersImportedDto>>
{
    public int ContactId { get; set; } = 1;
    public List<string>? ContactEmails { get; set; }
}
    
    
public class GetProcessesByContactQueryHandler: IRequestHandler<GetOrdersImportedByContactQuery, List<OrdersImportedDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrderImportedService _orderImportedService;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
        
    public GetProcessesByContactQueryHandler(
        IApplicationDbContext context,
        IOrderImportedService orderImportedService,
        IConfiguration configuration,
        IMapper mapper)
    {
        _context = context;
        _orderImportedService = orderImportedService;
        _configuration = configuration;
        _mapper = mapper;
    }
        
    public async Task<List<OrdersImportedDto>> Handle(GetOrdersImportedByContactQuery request, CancellationToken cancellationToken)
    {

        // BUSCAMOS LOS ORDERSIMPORTED DE LA TABLA DE ORDERS DE INTRANET
        var ordersImported =
            await _orderImportedService.GetOrdersImportedByContactId(request.ContactId, cancellationToken);

        // SOLO EN PRODUCCIÓN, SI NO NOS HAN ENVIADO DESDE EL FRONT UNA LISTA DE CORREOS, RETORNAMOS LAS ORDERSIMPORTED ENCONTADAS 
        if ( !_configuration.GetValue<bool>("IsProduction") || 
             request.ContactEmails is null || 
             request.ContactEmails.Count <= 0)
        {
            return ordersImported;
        }

        // SI NOS HAN ENVIADO DESDE EL FRONT UNA LISTA DE CORREOS
        var pedidosTlmk = new List<PedidoTlmk>();
        if (request.ContactEmails is not null && request.ContactEmails.Count > 0)
        {
            // BUSCAMOS LOS PEDIDOS TLMK POR ESOS CORREOS
            pedidosTlmk = await _orderImportedService.GetPedidoByContactEmails(request.ContactEmails, cancellationToken);
            
            // COMPARAMOS AMBAS LISTA PARA AVERIGUAR SI LOS PEDIDOS TLMK ENCONTRADOS YA ESTAN EN ORDERSIMPORTED
            var missingOrders = pedidosTlmk
                .Where(ptlmk => ordersImported.All(o => o.OrderNumber != ptlmk.NumPedido))
                .ToList();

            // SI HAY PEDIDOS TLMK QUE NO ESTAN EN LA TABLA DE ORDERS, LAS METEMOS
            if (missingOrders.Count > 0)
            {
                // TRANSFORMAMOS EL LISTADO DE PEDIDOS TLMK EN UN LISTA DE ORDERSIMPORTEDDTO
                var ordersImportedDtoFronTlmk =
                    _orderImportedService.GetOrdersImportedDtoListFromPedidoTlmkList(missingOrders, request.ContactId);
                // LOS AGREGAMOS A LA LISTA QUE VAMOS A DEVOLVER AL FRONT
                ordersImported.AddRange(ordersImportedDtoFronTlmk);
            
            
                // TRANSFORMAMOS EL LISTADO DE DTO ORDERSIMPORTEDDTO EN UN LISTADO DE LA ENTIDAD ORDERSIMPORTED
                // LO AGREGAMOS A LA TABLA Y GUARDAMOS
                List<IntranetMigrator.Domain.Entities.OrdersImported> ordersImportedFromTlmk = _mapper.Map<List<IntranetMigrator.Domain.Entities.OrdersImported>>(ordersImportedDtoFronTlmk);
                _context.OrdersImported.AddRange(ordersImportedFromTlmk);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        return ordersImported;
    }
}