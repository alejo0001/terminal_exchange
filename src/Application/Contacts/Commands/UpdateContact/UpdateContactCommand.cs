using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CrmAPI.Application.Contacts.Commands.UpdateContact;

public class UpdateContactCommand: ContactUpdateDto, IRequest
{
}
    
public class UpdateContactCommandHandler : IRequestHandler<UpdateContactCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateContactCommandHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly IIsDefaultService _isDefault;
    private readonly ICalendarService _calendar;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;
    private readonly IPotentialsService _potentialsService;

    public UpdateContactCommandHandler(IApplicationDbContext context, IMapper mapper, 
        ILogger<UpdateContactCommandHandler> logger, IIsDefaultService isDefault, IConfiguration configuration,
        ICalendarService calendar, ICurrentUserService currentUserService, IDateTime dateTime, IPotentialsService potentialsService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _isDefault = isDefault;
        _configuration = configuration;
        _currentUserService = currentUserService;
        _calendar = calendar;
        _dateTime = dateTime;
        _potentialsService = potentialsService;
    }
        
    public async Task<Unit> Handle(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        // BUSCAMOS EL CONTACTO
        var contact = await GetContactById(request.Id, cancellationToken);

        // ACTUALIZAMOS LOS DATOS BÁSICOS DEL CONTACTO
        UpdateBasicFieldsContact(request, contact);
        // ACTUALIZAMOS LAS ENTIDADES RELACIONADAS CON EL CONTACTO
        await UpdateEntitiesFieldsContact(request, cancellationToken, contact);
        // SI HACE FALTA, ELIMINAMOS LOS EVENTOS DEL CALENDARIO RELACIONADOS CON EL CONTACTO
        await RemoveEventCalendar(request, contact, cancellationToken).ConfigureAwait(false);

        // ACTUALIZAMOS LOS CAMBIOS Y MANDAMOS A ACTUALIZAR EL CONTACTO EN POTENCIALES
        await _context.SaveChangesAsync(cancellationToken);
        await _potentialsService.CreateOrUpdateContactInPotentials(contact.Id, cancellationToken, request.CouponOriginId);

        return Unit.Value;
    }

    private async Task<Contact?> GetContactById(int? contactId, CancellationToken cancellationToken)
    {
        return await _context.Contact
            .Include(c => c.Faculties)
            .Include(c => c.Specialities)
            .Include(c => c.ContactLeads.Where(cl => !cl.IsDeleted))
            .Include(c => c.Appointments.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == contactId, cancellationToken);
    }

    private async Task RemoveEventCalendar(UpdateContactCommand request, Contact contact, CancellationToken ct)
    {
        // Si no es un contacto válido, eliminamos todas sus citas
        if (request.ContactStatusId is not null && contact.ContactStatusId is 2 or > 3)
        {
            await _calendar.DeleteAllContactEvents(contact, ct).ConfigureAwait(false);
        }
    }

    private async Task UpdateEntitiesFieldsContact(UpdateContactCommand request, CancellationToken cancellationToken,
        Contact contact)
    {
        if (request.CurrencyCode is not null)
        {
            await UpdateCurrency(contact, request, cancellationToken);
        }

        // contact.Guid = request.Guid ?? contact.Guid; //TODO: revisar con Lorenzo

        if (request.Faculties is not null && request.Faculties.Count > 0)
        {
            await UpdateFaculties(contact, request, cancellationToken);
        }

        if (request.Specialities is not null && request.Specialities.Count > 0)
        {
            await UpdateSpecialities(contact, request, cancellationToken);
        }

        if (request.ContactPhone is not null && request.ContactPhone.Count > 0)
        {
            await UpdatePhones(request, cancellationToken);
        }

        if (request.ContactEmail is not null && request.ContactEmail.Count > 0)
        {
            await UpdateEmails(request, cancellationToken);
        }

        if (request.ContactAddress is not null && request.ContactAddress.Count > 0)
        {
            await UpdateAddresses(request, cancellationToken);
        }

        if (request.ContactLanguages is not null && request.ContactLanguages.Count > 0)
        {
            await UpdateLanguages(request, cancellationToken);
        }

        if (request.ContactTitles is not null && request.ContactTitles.Count > 0)
        {
            await UpdateTitles(request, cancellationToken);
        }
    }

    private static void UpdateBasicFieldsContact(UpdateContactCommand request, Contact contact)
    {
        if (request.Name is not null)
        {
            contact.Name = request.Name;
        }

        if (request.FirstSurName is not null)
        {
            contact.FirstSurName = request.FirstSurName;
        }

        if (request.SecondSurName is not null)
        {
            contact.SecondSurName = request.SecondSurName;
        }

        if (request.StudentCIF is not null)
        {
            contact.StudentCIF = request.StudentCIF;
        }

        if (request.FiscalCIF is not null)
        {
            contact.FiscalCIF = request.FiscalCIF;
        }

        if (request.LegalName is not null)
        {
            contact.LegalName = request.LegalName;
        }

        if (request.IdCard is not null)
        {
            contact.IdCard = request.IdCard;
        }

        if (request.ContactTypeId is not null)
        {
            contact.ContactTypeId = request.ContactTypeId ?? contact.ContactTypeId;
        }

        if (request.ContactStatusId is not null)
        {
            contact.ContactStatusId = request.ContactStatusId ?? contact.ContactStatusId;
        }

        if (request.CountryCode is not null)
        {
            contact.CountryCode = request.CountryCode;
        }

        if (request.Title is not null)
        {
            contact.Title = request.Title;
        }

        if (request.WorkCenter is not null)
        {
            contact.WorkCenter = request.WorkCenter;
        }

        if (request.DontWantCalls is not null)
        {
            contact.DontWantCalls = request.DontWantCalls;
        }

        if (request.RequestIp is not null)
        {
            contact.RequestIp = request.RequestIp;
        }

        if (request.Profession is not null)
        {
            contact.Profession = request.Profession;
        }

        if (request.ContactGenderId is not null)
        {
            contact.ContactGenderId = request.ContactGenderId ?? contact.ContactGenderId;
        }

        if (request.DateOfBirth is not null)
        {
            contact.DateOfBirth = request.DateOfBirth;
        }

        if (request.Nationality is not null)
        {
            contact.Nationality = request.Nationality;
        }
    }

    private async Task UpdateCurrency(Contact contact, UpdateContactCommand request, CancellationToken cancellationToken)
    {
        
        var currency = await _context.Currency
            .Where(c => c.CurrencyCode == request.CurrencyCode)
            .FirstOrDefaultAsync(cancellationToken);

        if (currency is not null)
        {
            contact.CurrencyId = currency.Id;
        }
    }


    private async Task UpdateFaculties(Contact contact, UpdateContactCommand request, CancellationToken cancellationToken)
    {
        if (request.Faculties is null || request.Faculties.Count <= 0)
        {
            return;
        }

        var facultiesId = (from faculty in request.Faculties
            where !contact.Faculties.Exists(f => f.Id == faculty.Id) select faculty.Id).ToList();

        var falculties = await _context.Faculties
            .Where(f => facultiesId.Contains(f.Id))
            .ToListAsync(cancellationToken);
        
        contact.Faculties.AddRange(falculties);
    }
    
    private async Task UpdateSpecialities(Contact contact, UpdateContactCommand request, CancellationToken cancellationToken)
    {
        if (request.Specialities is null || request.Specialities.Count <= 0)
        {
            return;
        }

        var specialitiesId = (from speciality in request.Specialities
            where !contact.Faculties.Exists(f => f.Id == speciality.Id) select speciality.Id).ToList();

        var specialities = await _context.Specialities
            .Where(s => specialitiesId.Contains(s.Id))
            .ToListAsync(cancellationToken);
        
        contact.Specialities.AddRange(specialities);
    }

    private async Task UpdatePhones(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        var phones = request.ContactPhone ?? new List<ContactPhoneUpdateDto>();
            
        if (phones.Any())
        {
            await _isDefault.SetIsDefault(request.ContactPhone);
                
            foreach (var phone in phones)
            {
                if (phone.Id != null)
                {
                    ContactPhone contactPhone = await _context.ContactPhone
                        .FirstOrDefaultAsync(ca => ca.Id == phone.Id, cancellationToken);
                    contactPhone.PhoneTypeId = phone.PhoneTypeId;
                    contactPhone.ContactId = phone.ContactId;
                    contactPhone.Phone = phone.Phone;
                    contactPhone.PhonePrefix = phone.PhonePrefix;
                    contactPhone.LastModified = _dateTime.Now;
                    contactPhone.LastModifiedBy = _currentUserService.Email;
                    contactPhone.IsDeleted = phone.IsDeleted;
                    contactPhone.IsDefault = phone.IsDefault;
                }
                else
                {
                    _context.ContactPhone.Add(_mapper.Map<ContactPhone>(phone));
                }
            }
        }
    }

    private async Task UpdateEmails(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        var emails = request.ContactEmail ?? new List<ContactEmailUpdateDto>();
            
        if (emails.Any())
        {
            await _isDefault.SetIsDefault(request.ContactEmail);
                
            foreach (var email in emails)
            {
                if (email.Id != null)
                {
                    ContactEmail contactEmail = await _context.ContactEmail
                        .FirstOrDefaultAsync(ca => ca.Id == email.Id, cancellationToken);
                    contactEmail.EmailTypeId = email.EmailTypeId;
                    contactEmail.ContactId = email.ContactId;
                    contactEmail.Email = email.Email;
                    contactEmail.LastModified = _dateTime.Now;
                    contactEmail.LastModifiedBy = _currentUserService.Email;
                    contactEmail.IsDeleted = email.IsDeleted;
                    contactEmail.IsDefault = email.IsDefault;
                }
                else
                {
                    _context.ContactEmail.Add(_mapper.Map<ContactEmail>(email));
                }
            }
        }
    }

    private async Task UpdateAddresses(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        var addresses = request.ContactAddress ?? new List<ContactAddressUpdateDto>();
           
            
        if (addresses.Any())
        {
            if (request.ContactAddress is not null)
            {
                await _isDefault.SetIsDefault(request.ContactAddress);    
            }

            foreach (var address in addresses)
            {
                var country = await _context.Country
                    .FirstOrDefaultAsync(c => c.CountryCode == address.CountryCode, cancellationToken);

                address.Province ??= "";

                if (address.Id != null)
                {
                    var contactAddress = await _context.ContactAddress
                        .FirstOrDefaultAsync(ca => ca.Id == address.Id, cancellationToken);
                    if (contactAddress is not null)
                    {
                        contactAddress.AddressTypeId = address.AddressTypeId;
                        contactAddress.ContactId = address.ContactId;
                        contactAddress.Address = address.Address;
                        contactAddress.City = address.City;
                        contactAddress.CountryId = country?.Id ?? int.Parse(_configuration["Constants:SpainCountryId"]);
                        contactAddress.Province = address.Province;
                        contactAddress.PostalCode = address.PostalCode;
                        contactAddress.Department = address.Department;
                        contactAddress.IsDeleted = address.IsDeleted;
                        contactAddress.IsDefault = address.IsDefault;
                    }
                }
                else
                {
                    var ca = _mapper.Map<ContactAddress>(address);
                    ca.CountryId = country?.Id ?? int.Parse(_configuration["Constants:SpainCountryId"]);
                    ca.LastModified = _dateTime.Now;
                    ca.LastModifiedBy = _currentUserService.Email;
                    _context.ContactAddress.Add(ca);
                }
            }
        }
    }

    private async Task UpdateLanguages(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        var languages = request.ContactLanguages ?? new List<ContactLanguageUpdateDto>();
            
        if (languages.Any())
        {
            bool hasDefault = false;
                
            foreach (var language in languages)
            {
                if (language.IsDefault && !hasDefault)
                {
                    hasDefault = true;
                }
                else
                {
                    language.IsDefault = request.ContactLanguages.Count == 1;
                }
                
                if (language.Id != null)
                {
                    var contactLanguage = await _context.ContactLanguages
                        .FirstOrDefaultAsync(ca => ca.Id == language.Id, cancellationToken);

                    if (contactLanguage is not null)
                    {
                        contactLanguage.ContactId = language.ContactId;
                        contactLanguage.LanguageId = language.LanguageId;
                        contactLanguage.IsDefault = language.IsDefault;
                        contactLanguage.IsDeleted = language.IsDeleted;    
                    }
                }
                else
                {
                    _context.ContactLanguages.Add(_mapper.Map<ContactLanguage>(language));
                }
            }
        }
    }

    private async Task UpdateTitles(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        var titles = request.ContactTitles ?? new List<ContactTitleUpdateDto>();
            
        if (titles.Any())
        {
            foreach (var title in titles)
            {
                if (title.Id != null)
                {
                    var contactTitle = await _context.ContactTitles
                        .FirstOrDefaultAsync(t => t.Id == title.Id, cancellationToken);
                    if (contactTitle is not null)
                    {
                        contactTitle.TitleTypeId = title.TitleTypeId;
                        contactTitle.AcademicInstitution = title.AcademicInstitution;
                        contactTitle.Degree = title.Degree;
                        contactTitle.IsDeleted = title.IsDeleted;
                    }
                }
                else
                {
                    _context.ContactTitles.Add(_mapper.Map<ContactTitle>(title));
                }
            }
        }
    }
        
}