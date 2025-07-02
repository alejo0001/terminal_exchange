using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Action = IntranetMigrator.Domain.Entities.Action;

namespace CrmAPI.Application.Processes.Commands.ExternalSuccessfulSaleProcess;

public class ExternalSuccessfulSaleProcessCommand: ExternalSaleDto, IRequest
{
}
    
public class ExternalSuccessfulSaleProcessCommandHandler : IRequestHandler<ExternalSuccessfulSaleProcessCommand>
{
    private readonly IApplicationDbContext _context;
   

    public ExternalSuccessfulSaleProcessCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(ExternalSuccessfulSaleProcessCommand request, CancellationToken cancellationToken)
    {

        var process = await _context.Processes
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == request.ProcessId, cancellationToken: cancellationToken);
        
        if (process is null)
        {
            return default;
        }

        // Desde web han pedido que se use el processId para obtener el contacto porque ellos no pueden mandar el contactId
        var contact = await _context.Contact
            .Include(c => c.ContactEmail)
            .Include(c => c.ContactAddress)
            .FirstOrDefaultAsync(c => c.Id == process.ContactId, cancellationToken);

        if (contact is null)
        {
            return default;
        }
        
        // ------ CONTACTO ------
        // TODO: Datos que se reciben pero no son compatibles para actualizar correctamente: Name, Phone

        if (contact.IdCard != request.IdCard
            && await _context.Contact.AllAsync(c => c.IdCard != request.IdCard, cancellationToken: cancellationToken))
        {
            contact.IdCard = request.IdCard;
        }
        
        if (!string.IsNullOrEmpty(request.Email)
            && contact.ContactEmail.All(e => e.Email != request.Email)
            && await _context.ContactEmail.AllAsync(e => e.Email != request.Email, cancellationToken))
        {
            contact.ContactEmail.Add(new ContactEmail()
            {
                Email = request.Email
            });
        }
        
        // TODO: quitar la vieja y poner la nueva dirección como default
        if (!string.IsNullOrEmpty(request.Address) &&
            contact.ContactAddress.All(a => a.PostalCode != request.PostalCode))
        {
            var country = await _context.Country.FirstOrDefaultAsync(c => c.CountryCode == request.Country, cancellationToken: cancellationToken);
            contact.ContactAddress.Add(new ContactAddress
            {
                Address = request.Address,
                Country = country,
                Province = request.Province,
                PostalCode = request.PostalCode,
                City = request.City
            });
        }
        
        // ------ ORDERSIMPORTED ------
        // TODO: revisar añadir mas campos
        var orderImported = new IntranetMigrator.Domain.Entities.OrdersImported
        {
            OrderNumber = request.OrderNumber,
            OrderDate = request.OrderDate,
            AcademicTitle = request.AcademicTitle,
            InitDate = request.InitDate,
            PaymentType = request.PaymentType,
            CurrencyCountry = request.CurrencyCountry,
            NumberDeadLines = request.NumberDeadLines,
            SalesCountry = request.SalesCountry,
            AmountRegistration = request.AmountRegistration
        };
        
        _context.OrdersImported.Add(orderImported);
        
        // ------ ACCIÓN DE VENTA ------
        _context.Actions.Add(new Action
        {
            FinishDate = DateTime.Now,
            Contact = contact,
            Process = process,
            User = process.User,
            OrdersImported = orderImported,
            Type = ActionType.WebSale,
            Outcome = ActionOutcome.Sale
        });
        
        process.Status = ProcessStatus.Closed;
        process.Outcome = ProcessOutcome.Sale;

        _context.Contact.Update(contact);
        _context.Processes.Update(process);
        await _context.SaveChangesAsync(cancellationToken);

        return default;
    }
}