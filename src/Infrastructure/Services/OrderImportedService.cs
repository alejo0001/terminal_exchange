using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using System.Linq;
using AutoMapper.QueryableExtensions;
using Domain.Tlmk.Entities;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Infrastructure.Services;

public class OrderImportedService: IOrderImportedService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ITlmkDbContext _tlmkDbContext;

    public OrderImportedService(
        IApplicationDbContext context,
        IMapper mapper,
        ITlmkDbContext tlmkDbContext
    )
    {
        _context = context;
        _mapper = mapper;
        _tlmkDbContext = tlmkDbContext;
    }

    public async Task<List<OrdersImportedDto>> GetOrdersImportedByContactId(int contactId, CancellationToken cancellationToken)
    {
       return await _context.OrdersImported
            .Where(o => o.ContactId == contactId && !o.IsDeleted)
            .ProjectTo<OrdersImportedDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PedidoTlmk>> GetPedidoByContactEmail(string email, CancellationToken cancellationToken)
    {
        return await _tlmkDbContext.PedidosTlmk
            .Where(p => p.Email == email)
            .Where(p => p.TipoPago != null
                        && !p.TipoPago.Contains("CANCELADO"))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PedidoTlmk>> GetPedidoByContactEmails(List<string> emails, CancellationToken cancellationToken)
    {
        return await _tlmkDbContext.PedidosTlmk
            .Where(p => emails.Contains(p.Email!))
            .Where(p =>  p.TipoPago != null && p.TipoPago.ToUpper().Trim() != "CANCELADO")
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PedidoTlmk>> GetPedidoByContactDni(string dni, CancellationToken cancellationToken)
    {
        return await _tlmkDbContext.PedidosTlmk
            .Where(p => p.Nif == dni)
            .ToListAsync(cancellationToken);
    }

    public List<OrdersImportedDto> GetOrdersImportedDtoListFromPedidoTlmkList(List<PedidoTlmk> pedidosTlmk, int contactId)
    {
        var ordersImportedFronTlmk = new List<OrdersImportedDto>();

        foreach (var pedidoTlmk in pedidosTlmk)
        {
            var orderImportedDtoFromPedidoTlmk = GetOrderImportedDtoFromPedidoTlmk(pedidoTlmk, contactId);
            orderImportedDtoFromPedidoTlmk.ImportedFromTlmk = true;
            ordersImportedFronTlmk.Add(orderImportedDtoFromPedidoTlmk);
        }

        return ordersImportedFronTlmk;
    }

    public async Task<List<int>> DeleteOrderImportedFromTlmkByEmail(List<string> emails, CancellationToken cancellationToken)
    {
        var ordersList = await _context.OrdersImported
            .Where(p => emails.Contains(p.Email!) && p.ImportedFromTlmk)
            .ToListAsync(cancellationToken);
        
        foreach (var order in ordersList)
        {
            order.IsDeleted = true;
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        return ordersList.Select(o => o.Id).ToList();
    }

    public OrdersImportedDto GetOrderImportedDtoFromPedidoTlmk(PedidoTlmk pedidoTlmk, int contactId)
    {
        return new OrdersImportedDto
        {
            OrderNumber = pedidoTlmk.NumPedido,
            CourseId = pedidoTlmk.Id_Curso,
            StudentNif = pedidoTlmk.Nif,
            StudentName = pedidoTlmk.Nombre,
            StudentSurName = pedidoTlmk.Apellidos,
            Address = pedidoTlmk.Direccion,
            BirthDate = pedidoTlmk.FechaNacimiento,
            PostsalCode = pedidoTlmk.Codpos,
            Province = pedidoTlmk.Provincia,
            Town = pedidoTlmk.Pobl,
            Country = pedidoTlmk.Pais,
            Phone = pedidoTlmk.Telefono,
            Email = pedidoTlmk.Email,
            OrderDate = pedidoTlmk.FechaPedido,
            Observations = pedidoTlmk.Observaciones,
            Title = pedidoTlmk.Titulo,
            Gender = pedidoTlmk.Sexo,
            WebUrl = pedidoTlmk.Web,
            Discount = pedidoTlmk.Descuento,
            AmountBase = pedidoTlmk.Precio,
            AmountRegistration = pedidoTlmk.Precio_Final,
            AmountDeadLines = pedidoTlmk.Precio_Plazos,
            NumberDeadLines = pedidoTlmk.Nplazos,
            FirstPaymentInEuro = pedidoTlmk.PrimerPagoEUR,
            Nationality = pedidoTlmk.Nacionalidad,
            Occupation = pedidoTlmk.Profesion,
            AcademicTitle = pedidoTlmk.Titulacion,
            University = pedidoTlmk.Universidad,
            Teleoperator = pedidoTlmk.Teleoperadora,
            RegistrationAgain = pedidoTlmk.Rematricula,
            Enterprise = pedidoTlmk.Empresa,
            InitDate = pedidoTlmk.FechaInicio,
            DurationCourseInDays = pedidoTlmk.DurationCourseInDays,
            Area = pedidoTlmk.Area,
            SalesCountry = pedidoTlmk.Pais_venta,
            CurrencyCountry = pedidoTlmk.Pais_moneda,
            EndorsementPersonId = pedidoTlmk.Id_avalista,
            StudentId = pedidoTlmk.IdStudent,
            Intensive = pedidoTlmk.Intensive == true ? 1 : 0,
            CourseCode = pedidoTlmk.CourseCode,
            Study = pedidoTlmk.Study,
            ProgramType = pedidoTlmk.ProgramType,
            InvoiceNumber = pedidoTlmk.IdFactura,
            TeamId = pedidoTlmk.IdEquipo,
            OrderOriginNumber = pedidoTlmk.NumPedidoOrigen,
            IsEnrollmentUpload = pedidoTlmk.IsEnrollmentUpload == true ? 1 : 0,
            AffiliateCode = pedidoTlmk.AffiliateCode ?? "",
            PaymentType = pedidoTlmk.TipoPago ?? "",
            ContactId = contactId
        };
    }    
}
