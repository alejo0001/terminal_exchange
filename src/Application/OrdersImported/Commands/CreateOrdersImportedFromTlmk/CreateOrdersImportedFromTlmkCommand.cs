using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NotificationAPI.Contracts.Commands;
using Action = IntranetMigrator.Domain.Entities.Action;

namespace CrmAPI.Application.OrdersImported.Commands.CreateOrdersImportedFromTlmk;

public class CreateOrdersImportedFromTlmkCommand : PedidoTlmkDto, IRequest<int>
{
    public string ApiKey { get; set; }
}

public class CreateOrdersImportedFromTlmkCommandHandler : IRequestHandler<CreateOrdersImportedFromTlmkCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IBus _bus;
    private readonly IMapper _mapper;
    private readonly IDateTime _dateTime;
    private readonly IConfiguration _configuration;

    public CreateOrdersImportedFromTlmkCommandHandler(IApplicationDbContext context, IBus bus,
        IMapper mapper, IDateTime dateTime, IConfiguration configuration)
    {
        _context = context;
        _bus = bus;
        _mapper = mapper;
        _dateTime = dateTime;
        _configuration = configuration;
    }

    public async Task<int> Handle(CreateOrdersImportedFromTlmkCommand request, CancellationToken cancellationToken)
    {

        var templateBody = "{\n numPedido: " + request.NumPedido;
        templateBody += "\n numPedidoAnterior: " + request.NumPedidoAnterior;
        templateBody += "\n numPedidoOriginal: " + request.NumPedidoOriginal;
        templateBody += "\n idCurso: " + request.IdCurso;
        templateBody += "\n idWeb: " + request.IdWeb;
        templateBody += "\n nif: " + request.Nif;
        templateBody += "\n nombre: " + request.Nombre;
        templateBody += "\n apellidos: " + request.Apellidos;
        templateBody += "\n direccion: " + request.Direccion;
        templateBody += "\n codpos: " + request.Codpos;
        templateBody += "\n provincia: " + request.Provincia;
        templateBody += "\n pobl: " + request.Pobl;
        templateBody += "\n pais: " + request.Pais;
        templateBody += "\n telefono: " + request.Telefono;
        templateBody += "\n email: " + request.Email;
        templateBody += "\n fechaPedido: " +request.FechaPedido;
        templateBody += "\n tipoPago: " + request.TipoPago;
        templateBody += "\n observaciones: " + request.Observaciones;
        templateBody += "\n titulo: " + request.Titulo;
        templateBody += "\n unidades: " + request.Unidades;
        templateBody += "\n precio: " + request.Precio;
        templateBody += "\n sexo: " + request.Sexo ;
        templateBody += "\n web: " + request.Web;
        templateBody += "\n descuento: " + request.Descuento;
        templateBody += "\n precioFinal: " + request.PrecioFinal;
        templateBody += "\n precioMatricula: " + request.PrecioMatricula;
        templateBody += "\n precioPlazos: " + request.PrecioPlazos;
        templateBody += "\n nplazos: " + request.Nplazos;
        templateBody += "\n nacionalidad: " + request.Nacionalidad;
        templateBody += "\n profesion: " + request.Profesion;
        templateBody += "\n titulacion: " + request.Titulacion;
        templateBody += "\n universidad: " + request.Unidades;
        templateBody += "\n teleoperadora: " + request.Teleoperadora;
        templateBody += "\n ntarjeta: " + request.Ntarjeta;
        templateBody += "\n tokenPedido: " + request.TokenPedido;
        templateBody += "\n ncuenta: " + request.Ncuenta;
        templateBody += "\n rematricula: " + request.Rematricula;
        templateBody += "\n empresa: " + request.Empresa;
        templateBody += "\n refRedsys: " + request.RefRedsys;
        templateBody += "\n fechaNacimiento: " + request.FechaNacimiento;
        templateBody += "\n fechaInicio: " + request.FechaInicio;
        templateBody += "\n area: " + request.Area;
        templateBody += "\n paisVenta: " + request.PaisVenta;
        templateBody += "\n paisMoneda: " + request.PaisMoneda;
        templateBody += "\n idAvalista: " + request.IdAvalista;
        templateBody += "\n idStudent: " + request.IdStudent;
        templateBody += "\n intensive: " + request.Intensive;
        templateBody += "\n affiliateCode: " + request.AffiliateCode;
        templateBody += "\n affiliateComissionPercent: " + request.AffiliateComissionPercent;
        templateBody += "\n promotionalCode: " + request.PromotionalCode;
        templateBody += "\n courseCode: " + request.CourseCode;
        templateBody += "\n study: " + request.Study;
        templateBody += "\n programType: " + request.ProgramType;
        templateBody += "\n codigoPedidoRedsys: " + request.CodigoPedidoRedsys;
        templateBody += "\n importeCobroRedsys: " + request.ImporteCobroRedsys;
        templateBody += "\n plataformaPago: " + request.PlataformaPago;
        templateBody += "\n duracion: " + request.Duracion;
        templateBody += "\n fechaFin: " + request.FechaFin;
        templateBody += "\n creditos: " + request.Creditos;
        templateBody += "\n primerPagoEUR: " + request.PrimerPagoEUR;
        templateBody += "\n primerPago: " + request.PrimerPago;
        templateBody += "\n divisaPrimerPago: " + request.DivisaPrimerPago;
        templateBody += "\n ratio: " + request.Ratio;
        templateBody += "\n idFactura: " + request.IdFactura;
        templateBody += "\n politicasPrivacidadAceptadas: "  + request.PoliticasPrivacidadAceptadas;
        templateBody += "\n condicionesContratacionAceptadas: " + request.CondicionesContratacionAceptadas;
        templateBody += "\n isRenewal: " + request.IsRenewal;
        templateBody += "\n importeRenovacion: " + request.ImporteRenovacion;
        templateBody += "\n numPedidoOrigen: " + request.NumPedido;
        templateBody += "\n clientNotificationsSent: " + request.ClientNotificationsSent;
        templateBody += "\n isEnrollmentUpload: " + request.IsEnrollmentUpload;
        templateBody += "\n durationCourseInDays: " + request.DurationCourseInDays;
        templateBody += "\n idPago: " + request.IdPago;
        templateBody += "\n idIdioma: " + request.IdIdioma;
        templateBody += "\n idioma: " + request.Idioma;
        templateBody += "\n tituloIdioma: " + request.TituloIdioma;
        templateBody += "\n areaIdioma: " + request.AreaIdioma;
        templateBody += "\n estudioIdioma: " + request.EstudioIdioma;
        templateBody += "\n processId: " + request.ProcessId;
        templateBody += "\n apiKey: " + request.ApiKey;
        templateBody += "\n }: ";
        
        
        
        
        var emailContract = new CreateEmail
        {
            CorrelationId = Guid.NewGuid(),
            From = "intranet@techtitute.com",
            Subject = "Nuevo Pedido desde TLMK recibido en CRM",
            Receivers = new List<string>
            {
                "joghernandez@techtitute.com"
            },
            Body = templateBody,
            BccReceivers = new List<string>(),
            UseExchange = false
        };
        
        await _bus.Publish(emailContract, cancellationToken);
        
        
        

        // Buscamos el orderImported por si existiera. Si es asi, no hacemos nada.
        if (request.ProcessId is not null)
        {
            var duplicatedOrder = await _context.OrdersImported
                .Where(p => p.ProcessId == request.ProcessId)
                .FirstOrDefaultAsync(cancellationToken);

            if (duplicatedOrder != null)
            {
                return 0;    
            }
        }

        // Mapeamos el request a un OrdersImported
        IntranetMigrator.Domain.Entities.OrdersImported ordersImported = _mapper.Map<IntranetMigrator.Domain.Entities.OrdersImported>(request);

        // Buscamos el proceso correspondiente que nos llega
        var process = await GetProcess(ordersImported, cancellationToken);

        if (process == null)
        {
            
            var emailContractNotFound = new CreateEmail
            {
                CorrelationId = Guid.NewGuid(),
                From = "intranet@techtitute.com",
                Subject = $"El proceso para el email {ordersImported.Email} o el ID: {ordersImported.ProcessId} No ha sido encontrado",
                Receivers = new List<string>
                {
                    "joghernandez@techtitute.com"
                },
                Body = templateBody,
                BccReceivers = new List<string>(),
                UseExchange = false
            };
        
            await _bus.Publish(emailContractNotFound, cancellationToken);
            
            
            return 0;
        }
        
        _context.OrdersImported.Add(ordersImported);
        // Guardamos los cambios para poder aceder al Id del OrderImported sin problemas mas adelante
        await _context.SaveChangesAsync(cancellationToken);
        
        ordersImported.CountryId = await GetCountryId(ordersImported.SalesCountry, cancellationToken);
        ordersImported.CurrencySaleCountryId = await GetCurrencyCountryId(ordersImported, cancellationToken);
        ordersImported.ProcessId = process.Id;


        // Establecemos en el order imported el contactId y lo agregamos a la base de datos
        ordersImported.ContactId = process.ContactId;
        ordersImported.StudentSurName = $"{process.Contact.FirstSurName} {process.Contact.SecondSurName}"; 
        
        // Eliminamos el curso que hemos vendido de los contactLeads del contacto (cursos de interés)
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Title == ordersImported.Title, cancellationToken);

        if (course != null)
        {
            var contactLead = await _context.ContactLeads
                .Where(cl => cl.ContactId == process.ContactId && cl.CourseId == course.Id)
                .FirstOrDefaultAsync(cancellationToken);
            
            if (contactLead is not null)
            {
                contactLead.IsDeleted = true;
            }

            // Confirmamos todos los cambios a la base de datos
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Cambiamos el proceso a estado pendiente (pendiente de pago) y con la salida "Sale"
        process.Status = ProcessStatus.Pending;
        process.Outcome = ProcessOutcome.PaymentMethodPending;
        process.OrdersImported = ordersImported;

        if (process.Contact == null)
        {
            return 0;
        }

        process.Contact.NextInteraction = null;

        var newAction = new Action
        {
            UserId = process.UserId,
            ContactId = process.ContactId,
            User = process.User,
            Date = _dateTime.Now,
            FinishDate = _dateTime.Now,
            ProcessId = process.Id,
            Type = ActionType.Sale,
            Outcome = ActionOutcome.Sale,
            OrdersImported = ordersImported,
            OrdersImportedId = ordersImported.Id
        };
            
        _context.Actions.Add(newAction);
            
        var callInCourse = await _context.Actions
            .Where(a => a.ProcessId == process.Id &&
                        a.ContactId == process.ContactId &&
                        a.Type == ActionType.Call &&
                        a.FinishDate == null)
            .FirstOrDefaultAsync(cancellationToken);
        if (callInCourse != null)
        {
            callInCourse.OrdersImported = ordersImported;
        }


        await _context.SaveChangesAsync(cancellationToken);

        return ordersImported.Id;
    }

    private async Task<int> GetCurrencyCountryId(IntranetMigrator.Domain.Entities.OrdersImported ordersImported, CancellationToken cancellationToken)
    {
        var currencyCountry = await _context.Country
            .FirstOrDefaultAsync(c => c.CountryCode == ordersImported.CurrencyCountry, cancellationToken);

        if (currencyCountry is null)
        {
            currencyCountry = await _context.Country
                .FirstOrDefaultAsync(c => c.CountryCode == _configuration["Constants:SpainCountryCode"], cancellationToken);
        }

        return currencyCountry!.Id;
    }

    private async Task<Process?> GetProcess(IntranetMigrator.Domain.Entities.OrdersImported ordersImported, CancellationToken cancellationToken)
    {
        var process = await _context.Processes
            .Include(p => p.User)
            .Include(p => p.Contact)
            .Where(p => p.Id == ordersImported.ProcessId)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (process is null)
        {
            process = await _context.Processes
                .Include(p => p.User)
                .Include(p => p.Contact)
                .Where(p => p.Contact.ContactEmail.Any(ce => ce.Email.Trim() == ordersImported.Email.Trim() && !ce.IsDeleted))
                .Where(p => p.User.Employee.IdVendedor.ToString() == ordersImported.Teleoperator)
                .Where(p => p.Status != ProcessStatus.Closed)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return process;
    }

    private async Task<int> GetCountryId(string salesCountry, CancellationToken cancellationToken)
    {
        var country = await _context.Country
            .Where(c => c.CountryCode == salesCountry)
            .FirstOrDefaultAsync(cancellationToken);

        if (country == null)
        {
            country = await _context.Country
                .Where(c => c.CountryCode == _configuration["Constants:SpainCountryCode"])
                .FirstOrDefaultAsync(cancellationToken);
        }


        return country!.Id;
    }
}