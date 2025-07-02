using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Queries.GetContactInfoForTlmk;

public class GetContactInfoForTlmkQuery : IRequest<ContactInfoTlmkDto>
{
    public int ProcessId { get; set; }
    public string ApiKey { get; set; }
}

// ESTE COMANDO ENVÍA LA INFORAMCIÓN DEL CONTACTO AL TLMK. ES UNA LLAMADA QUE HACE EL TLMK
// PARA AUTORRELLENAR EL FORMULARIO CON LOS DATOS DEL CONTACTO
public class GetContactInfoForTlmkQueryHandler : IRequestHandler<GetContactInfoForTlmkQuery, ContactInfoTlmkDto>
{
    private readonly IApplicationDbContext _context;

    private readonly IMapper _mapper;

    public GetContactInfoForTlmkQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ContactInfoTlmkDto> Handle(GetContactInfoForTlmkQuery request,
        CancellationToken cancellationToken)
    {
        var process = await _context.Processes
            .Include(p => p.Contact)
            .ThenInclude(c => c.ContactAddress.Where(ca => ca.IsDefault == true && !ca.IsDeleted))
            .ThenInclude(ca => ca.Country)
            .Include(p => p.Contact)
            .ThenInclude(c => c.ContactPhone.Where(ca => ca.IsDefault == true && !ca.IsDeleted))
            .Include(p => p.Contact)
            .ThenInclude(c => c.ContactEmail.Where(ca => ca.IsDefault == true && !ca.IsDeleted))
            .Include(p => p.Contact)
            .ThenInclude(c => c.ContactTitles.Where(ca => !ca.IsDeleted))
            .ThenInclude(ct => ct.TitleType)
            .FirstOrDefaultAsync(p => p.Id == request.ProcessId, cancellationToken);
        if (process is null)
        {
            throw new NotFoundException(nameof(Process), request.ProcessId);
        }
            
        // DTO construction
        Contact contact = process.Contact;
        ContactInfoTlmkDto dto = _mapper.Map<ContactInfoTlmkDto>(contact);

        dto.Gender = ((ContactGenderEnum)contact.ContactGenderId).ToString().ToLowerInvariant();
        ContactAddress ca = contact.ContactAddress.FirstOrDefault();
        dto.Address = ca?.Address ?? "";
        dto.City = ca?.City ?? "";
        dto.Department = ca?.Department ?? "";
        dto.Province = ca?.Province ?? "";
        dto.CountryCode = ca?.Country?.CountryCode ?? contact.CountryCode;
        dto.PostalCode = ca?.PostalCode ?? "";
        ContactTitle ct = contact.ContactTitles.FirstOrDefault();
        dto.AcademicInstitution = ct?.AcademicInstitution ?? ""; 
        dto.TitleType = ct?.TitleType?.Name ?? "";
        dto.Nationality = contact.Nationality;
            
        // Save SaleAttempts
        process.SaleAttempts ??= 0;
        process.SaleAttempts++;
        await _context.SaveChangesAsync(cancellationToken);
            
        return dto;
    }
}