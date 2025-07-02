using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CroupierAPI.Contracts.Commands;
using CroupierAPI.Contracts.Events;
using IntranetMigrator.Domain.Entities;
using MassTransit.Futures.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CrmAPI.Application.Contacts.Queries.GetContactByEmailOrPhone;

public class GetContactByEmailOrPhoneQuery: IRequest<ContactGetted>
{
    public  GetContact  Data { get; set; }
}

public class GetContactByEmailOrPhoneQueryHandler : IRequestHandler<GetContactByEmailOrPhoneQuery, ContactGetted>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    public GetContactByEmailOrPhoneQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ContactGetted> Handle(GetContactByEmailOrPhoneQuery request, CancellationToken cancellationToken)
    {
        var contact = new Contact();

        if (request.Data.Email is not null)
        {
            contact = await _context.Contact
                .Include(c => c.ContactEmail)
                .Include(c => c.ContactPhone)
                .Where(c => c.ContactEmail.Any(ce => ce.Email == request.Data.Email && !ce.IsDeleted) && !c.IsDeleted)
                .FirstOrDefaultAsync();
        }

        //
        // // CASO SENCILLO DE COMPARACIÓN DE TELÉFONOS
        // if ((contact == null || contact.Id == 0) && request.Data.Phone is not null)
        // {
        //     var phonePrefix = request.Data.Phone.Replace(" ", "").Replace("+", "");
        //     
        //     contact = await _context.Contact
        //         .Include(c => c.ContactPhone)
        //         .Include(c => c.ContactEmail)
        //         .Where(c => c.ContactPhone.Any(cp => cp.Phone.Replace(" ", "").Replace("+", "").Replace("(", "").Replace(")", "") == phonePrefix && !cp.IsDeleted) && !c.IsDeleted)
        //         .FirstOrDefaultAsync();
        // }
        //
        //
        // var prefix = await _context.Country.Where(c => c.CountryCode == request.Data.CountryCode)
        //     .FirstOrDefaultAsync(cancellationToken);
        //
        //
        // // CASOS EN LOS QUE TENEMOS EL TELÉFONO DEL CONTACTO CON EL PREFIJO GUARDADO, PERO ÉL LO HA PUESTO SIN PREFIJO. NOS BASAMOS EN EL PAÍS
        // if ((contact == null || contact.Id == 0) && request.Data.Phone is not null)
        // {
        //     if (prefix is not null)
        //     {
        //         var phonePrefix = $"{prefix.PhonePrefix}{request.Data.Phone}".Replace(" ", "").Replace("+", "").Replace("(", "").Replace(")", "");  
        //     
        //         contact = await _context.Contact
        //             .Include(c => c.ContactPhone)
        //             .Include(c => c.ContactEmail)
        //             .Where(c => c.ContactPhone.Any(cp => cp.Phone.Replace(" ", "").Replace("+", "") == phonePrefix && !cp.IsDeleted) && !c.IsDeleted)
        //             .FirstOrDefaultAsync();
        //
        //         if ((contact == null || contact.Id == 0))
        //         {
        //             phonePrefix = phonePrefix.Replace(" ", "").Replace("+", "").Replace("(", "").Replace(")", "");  
        //     
        //             contact = await _context.Contact
        //                 .Include(c => c.ContactPhone)
        //                 .Include(c => c.ContactEmail)
        //                 .Where(c => c.ContactPhone.Any(cp => cp.Phone.Replace(" ", "").Replace("+", "") == phonePrefix && !cp.IsDeleted) && !c.IsDeleted)
        //                 .FirstOrDefaultAsync();
        //         }
        //     }
        // }
        //
        //
        // // SI EL CONTACTO HA PUESTO EL PREFIJO TENEMOS QUE COMPARAR LOS CAMPOS PREFIJO Y TELÉFONO
        // if ((contact == null || contact.Id == 0) && request.Data.Phone is not null)
        // {
        //     var phonePrefix = request.Data.Phone.Replace(" ", "").Replace("+", "").Replace("(", "").Replace(")", "");
        //
        //    
        //
        //     contact = await _context.Contact
        //         .Include(c => c.ContactPhone)
        //         .Include(c => c.ContactEmail)
        //         .Where(c => c.ContactPhone.Any(cp => (cp.PhonePrefix + cp.Phone).Replace(" ", "").Replace("+", "").Replace("(", "").Replace(")", "")  == phonePrefix && !cp.IsDeleted) && !c.IsDeleted)
        //         .FirstOrDefaultAsync(cancellationToken);
        // }
        //
        // // SI EL CONTACTO HA PUESTO EL PREFIJO CON CEROS
        // if ((contact == null || contact.Id == 0) && request.Data.Phone is not null)
        // {
        //     var phonePrefix = request.Data.Phone.Replace(" ", "").Replace("+", "").Replace("(", "").Replace(")", "").TrimStart('0');
        //
        //     contact = await _context.Contact
        //         .Include(c => c.ContactPhone)
        //         .Include(c => c.ContactEmail)
        //         .Where(c => c.ContactPhone.Any(cp => (cp.PhonePrefix + cp.Phone).Replace(" ", "").Replace("+", "").Replace("(", "").Replace(")", "")  == phonePrefix && !cp.IsDeleted) && !c.IsDeleted)
        //         .FirstOrDefaultAsync(cancellationToken);
        //
        //     if (contact == null || contact.Id == 0)
        //     {
        //         //TODO FALTA AQUI LIMPIEAR LOS CEROS, PERO NO ADMITE EL TRIMSTART, DA ERROR
        //
        //         contact = await _context.Contact
        //             .Include(c => c.ContactPhone)
        //             .Include(c => c.ContactEmail)
        //             .Where(c => c.ContactPhone.Any(cp => (cp.PhonePrefix + cp.Phone).Replace(" ", "").Replace("+", "").Replace("(", "").Replace(")", "")  == phonePrefix && !cp.IsDeleted) && !c.IsDeleted)
        //             .FirstOrDefaultAsync(cancellationToken);
        //     }
        // }
        //
        // // SI TENEMOS EN EL CAMPO DE TELÉFONO EL PREFIJO Y NÚMERO GUARDADO
        // if ((contact == null || contact.Id == 0) && request.Data.Phone is not null)
        // {
        //     var phonePrefix = request.Data.Phone.Replace(" ", "").Replace("+", "").Replace("(", "").Replace(")", "").TrimStart('0');
        //     
        //     contact = await _context.Contact
        //         .Include(c => c.ContactPhone)
        //         .Include(c => c.ContactEmail)
        //         .Where(c => c.ContactPhone.Any(cp => cp.Phone.Replace(" ", "").Replace("+", "").Replace("(", "").Replace(")", "")  == phonePrefix && !cp.IsDeleted) && !c.IsDeleted)
        //         .FirstOrDefaultAsync(cancellationToken);
        // }

        if (contact is null)
        {
            return new ContactGetted()
            {
                CorrelationId = request.Data.CorrelationId,
                Id = 0,
                Name = "",
                FirstSurName = "",
                SecondSurName = ""
            };
        } 

        var contactGetted = _mapper.Map<ContactGetted>(contact);
         contactGetted.CorrelationId = request.Data.CorrelationId;

         return contactGetted;
    }
}