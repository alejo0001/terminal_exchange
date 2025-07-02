using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CrmAPI.Application.Contacts.Queries.GetContactDetails;

[Authorize(Roles = "Usuario")]
public class GetContactDetailsQuery : IRequest<ContactFullDto>
{
    public int ContactId { get; set; }
    public int ProcessId { get; set; }
}
    
public class GetContactDetailsQueryHandler : IRequestHandler<GetContactDetailsQuery, ContactFullDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly ICalendarService _calendar;
    private readonly ILeadsDbContext _iLeadsDbContext;

    public GetContactDetailsQueryHandler(IApplicationDbContext context, IConfiguration configuration, IMapper mapper, ICalendarService calendar, ILeadsDbContext iLeadsDbContext)
    {
        _context = context;
        _configuration = configuration;
        _mapper = mapper;
        _calendar = calendar;
        _iLeadsDbContext = iLeadsDbContext;
    }
        
    public async Task<ContactFullDto> Handle(GetContactDetailsQuery request, CancellationToken cancellationToken)
    {
        //TODO: refactorizar esta consulta zurda
        var process = await _context.Processes
            .Include(p => p.OrdersImported)
                .ThenInclude(o => o.BusinessCountry)
                    .ThenInclude(c => c.Currency)
            .Include(p => p.DiscardReasonProcess)
                .ThenInclude(dp => dp.DiscardReason)
            .Include(p => p.OrdersImported)
                .ThenInclude(o => o.CurrencySaleCountry)
                    .ThenInclude(c => c.Currency)
            .Where(p => p.ContactId == request.ContactId && p.Id == request.ProcessId)
            .FirstOrDefaultAsync(cancellationToken);

        if (process is null)
        {
            throw new NotFoundException("Process not found!");
        }


        var contact = await _context.Contact
          //  .Include(c => c.Appointments)
            .Include(c => c.InvoicePaymentOptions.Where(i => !i.IsDeleted))
            .Include(c => c.ContactAddress.Where(i => !i.IsDeleted))
                .ThenInclude(a => a.AddressType )
            .Include(c => c.ContactAddress.Where(i => !i.IsDeleted))
                .ThenInclude(c => c.Country)
                    .ThenInclude(cu => cu.Currency)
          .Include(s => s.Status)
            .Include(c => c.ContactTitles.Where(ct => !ct.IsDeleted))
            .Include(c => c.ContactLanguages.Where(ct => !ct.IsDeleted))
                .ThenInclude(cl => cl.Language)
          .AsSplitQuery()
            .Where(c => c.Id == request.ContactId)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (contact is null)
        {
            throw new NotFoundException("Contact not found!");
        }


        //TODO: esto habría que hacerlo cdo se cierra el proceso. (Andrew)
     /*  if (contact.NextInteraction is null  
            && process.Status is ProcessStatus.Pending or ProcessStatus.Closed
            && contact.Appointments is not null 
            && contact.Appointments.Any(a => !a.IsDeleted))
        {
            await _calendar.DeleteAllContactEvents(contact);
            await _context.SaveChangesAsync(cancellationToken);
        }*/


        var dto = _mapper.Map<ContactFullDto>(contact);
        dto.SaleAttempts = process.SaleAttempts;
        dto.Processes = new List<ProcessChildViewModel> { _mapper.Map<ProcessChildViewModel>(process) };

        if (dto.CountryCode is null)
        {
            dto.CountryCode = _configuration["Constants:SpainCountryCode"];

            contact.CountryCode = _configuration["Constants:SpainCountryCode"];

            await _context.SaveChangesAsync(cancellationToken);
        }
        
        if (dto.CurrencyId is null)
        {
        
            var currency = await _context.Currency
                .Where(c => c.CurrencyCode == _configuration["Constants:DefaultCurrencyCode"])
                .FirstOrDefaultAsync(cancellationToken);
        
            if (currency is not null)
            {

                contact.CurrencyId = currency.Id;
                
                dto.CurrencyId = currency.Id;
                dto.Currency = _mapper.Map<CurrencyDto>(currency);

                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        
        dto.Country = await GetSaleCountry(dto, cancellationToken);
        dto.Currency = await GetSaleCurrency(dto, cancellationToken);

        return dto;
    }

    private async Task<CourseCountryDto> GetSaleCountry(ContactFullDto contactFullDto, CancellationToken cancellationToken)
    {

        var languageCode = _configuration["Constants:DefaultLanguageCode"];
        var contactLanguageCode = contactFullDto.ContactLanguages.Find(cl => cl.IsDefault) ?? null;
        var contactCountryCode = contactFullDto.CountryCode;

        if (contactLanguageCode is null)
        {
            if (contactFullDto.ContactLanguages.Count > 0)
            {
                contactLanguageCode = contactFullDto.ContactLanguages.First();    
            }
        }

        if (contactLanguageCode is not null)
        {
            languageCode = contactLanguageCode.Language.Name;
        }
        
        var courseCountry = await _context.CourseCountries
            .Include(cc => cc.Language)
            .Where(cc => cc.Code == contactCountryCode && cc.LanguageCode == languageCode)
            .FirstOrDefaultAsync(cancellationToken);

        if (courseCountry is not null)
        {
            courseCountry.CurrencyFormat = (await _context.Currency
                .Where(c => c.CurrencyCode == courseCountry.CurrencyCode)
                .FirstOrDefaultAsync(cancellationToken))?.CurrencyDisplayFormat ?? courseCountry.CurrencyFormat;
        }

        return _mapper.Map<CourseCountryDto>(courseCountry);
    }

    private async Task<CurrencyDto> GetSaleCurrency(ContactFullDto contactFullDto, CancellationToken cancellationToken)
    {

        if (contactFullDto.Currency is not null)
        {
            return contactFullDto.Currency;
        }
        
        var currency = await _context.Currency
            .Where(c => c.Id == contactFullDto.CurrencyId)
            .FirstOrDefaultAsync(cancellationToken);

        return _mapper.Map<CurrencyDto>(currency);
    }

}