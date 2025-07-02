using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Contacts.Commands.UpdateContact;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CrmAPI.Infrastructure.Services;

public class PotentialsService : IPotentialsService
{
    private readonly ICroupierApiClient _croupierApiClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PotentialsService> _logger;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public PotentialsService(
        ICroupierApiClient croupierApiClient,
        IConfiguration configuration,
        ILogger<PotentialsService> logger,
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _croupierApiClient = croupierApiClient;
        _configuration = configuration;
        _logger = logger;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task CreateOrUpdateContactInPotentials(int contactId,  CancellationToken cancellationToken,  [Optional]int? couponOriginId)
    {

         // SE DESHABILITA MOMENTANEAMENTE PARA EVITAR LA LENTITUD DE CROUPIER
         if (!_configuration.GetValue<bool>("IsProduction"))
         {
             return;
         }


         var contact = await _context.Contact
            .Include(c => c.Faculties)
            .Include(c => c.Specialities)
            .Include(c => c.ContactEmail.Where(e => !e.IsDeleted))
            .Include(c => c.ContactLanguages.Where(cl => !cl.IsDeleted))
            .Include(c => c.ContactPhone.Where(p => p.IsDeleted == false))
            .Include(c => c.ContactLeads.Where(cl => !cl.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == contactId, cancellationToken);

        if (contact is not null)
        {
            var lead = new LeadDto
            {
                ContactId = contact.Id,
                ContactGuid = contact.Guid,
                Name = contact.Name,
                FirstSurname = contact.FirstSurName,
                SecondSurname = contact.SecondSurName,
                Address = "",
                City = "",
                Province = "",
                PostalCode = "",
                Nif = contact.IdCard,
                Created = contact.Created,
                ContactGenderId = contact.ContactGenderId,
                ContactStatusId = contact.ContactStatusId,
                CountryCode = contact.CountryCode,
                NationalityCode = contact.Nationality,
                LanguagesId = new List<int>(),
                FacultiesId = contact.Faculties.Select(f => f.Id).ToList(),
                SpecialitiesId = contact.Specialities.Select(s => s.Id).ToList(),
                CoursesId = new List<int?>(),
                Emails = contact.ContactEmail.Where(ce => ce.IsDeleted == false).Select(ce => ce.Email).ToList(),
                Phones = contact.ContactPhone.Where(cp => cp.IsDeleted == false).Select(cp => cp.PhonePrefix + cp.Phone).ToList()
            };

            lead.Provenance = await GetProvenance(couponOriginId ?? 0, cancellationToken);


            if (contact.ContactAddress is not null)
            {
                lead.Address = contact.ContactAddress.Where(ca => (bool)ca.IsDefault).Select(ca => ca.Address).FirstOrDefault();
                lead.City = contact.ContactAddress.Where(ca => (bool)ca.IsDefault).Select(ca => ca.City).FirstOrDefault();
                lead.Province = contact.ContactAddress.Where(ca => (bool)ca.IsDefault).Select(ca => ca.Province).FirstOrDefault();
                lead.PostalCode = contact.ContactAddress.Where(ca => (bool)ca.IsDefault).Select(ca => ca.PostalCode).FirstOrDefault();
            }

            if (contact.ContactLeads is not null)
            {
                lead.CoursesId = contact.ContactLeads.Where(cl => cl.IsDeleted == false && cl.CourseId is not null).Select(cl => cl.CourseId).ToList();
            }

            if (contact.ContactLanguages is not null)
            {
                lead.LanguagesId = contact.ContactLanguages.Select(cl => cl.LanguageId).ToList();
            }
            else
            {
                var employee = await _context.Employees.Where(e => e.CorporateEmail == _currentUserService.Email)
                    .FirstOrDefaultAsync(cancellationToken);

                if (employee is not null)
                {
                    var language = await _context.Languages.Where(l => l.Name == employee.Nationality)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (language == null)
                    {
                        language = await _context.Languages.Where(l => l.Name == _configuration["Constants:DefaultLanguageCode"])
                            .FirstOrDefaultAsync(cancellationToken);
                    }

                    if (language != null)
                    {
                        lead.LanguagesId.Add(language.Id);
                    }
                } else {

                    var language = await _context.Languages.Where(l => l.Name == _configuration["Constants:DefaultLanguageCode"])
                        .FirstOrDefaultAsync(cancellationToken);

                    if (language != null)
                    {
                        lead.LanguagesId.Add(language.Id);
                    }
                }
            }

            // TODO: Possible Crm optimization.
            // This thing is weird. Why updating Contact info in "potenciales" DB cannot be a responsibility of CRM?
            // Right now it is don in CroupierAPI with following, but it seems off in overall Intranet architecture.
            try
            {
                var response = await _croupierApiClient.UpdateContactFromIntranet(lead, cancellationToken)
                    .ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Error connect with croupier endpoint UpdateContactFromIntranet");
            }
        }
    }

    private async Task<string> GetProvenance(int couponOriginId, CancellationToken cancellationToken)
    {
        var provenance = "crm";

        if (couponOriginId == 0)
        {
            return provenance;
        }

        var couponOrigen = await _context.CouponOrigins.Where(c => c.Id == couponOriginId)
            .FirstOrDefaultAsync(cancellationToken);

        if (couponOrigen is not null)
        {
            provenance = $"{provenance}-{couponOrigen.Name}";
        }

        return provenance;
    }

    public async Task UpdateStatusContactInPotentials(int originalContactId, int status, CancellationToken ct)
    {
        // TODO: REPLICAR LA BASA EN LOS DEMÁS SISTEMAS.
        // TODO: Possible Crm optimization.
        // This thing is weird. Why updating Contact info in "potenciales" DB cannot be a responsibility of CRM?
        // Right now it is don in CroupierAPI with following, but it seems off in overall Intranet architecture.
        try
        {
            var response = await _croupierApiClient.UpdateContactStatusFromIntranet(originalContactId, status, ct)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
        }
        catch
        {
            throw new NotFoundException("Failed connection");
        }
    }
}
