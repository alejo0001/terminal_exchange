using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrmAPI.Application.Contacts.Queries.GetContactLeadsByContactAndProcess;

public class GetContactLeadsByContactAndProcessQuery : IRequest<List<ContactLeadDto>>
{
    public int ContactId { get; set; }
    public int ProcessId { get; set; }
}

[UsedImplicitly]
public class GetContactLeadsByContactAndProcessQueryHandler : IRequestHandler<GetContactLeadsByContactAndProcessQuery, List<ContactLeadDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICourseUnApiClient _courseUnApiClient;
    private readonly IMapper _mapper;
    private readonly ILogger<GetContactLeadsByContactAndProcessQueryHandler> _logger;
    private readonly IConfiguration _configuration;

    public GetContactLeadsByContactAndProcessQueryHandler(
        IApplicationDbContext context,
        ICourseUnApiClient courseUnApiClient,
        IMapper mapper,
        ILogger<GetContactLeadsByContactAndProcessQueryHandler> logger,
        IConfiguration configuration)
    {
        _context = context;
        _courseUnApiClient = courseUnApiClient;
        _mapper = mapper;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<List<ContactLeadDto>> Handle(GetContactLeadsByContactAndProcessQuery request, CancellationToken cancellationToken)
    {
        var contactLeadList =  await GetContactLeadDtosAsync(request, cancellationToken);

        /*  List<ContactLead> leadsNoPriceList = contactLeadList
              .Where(cl => cl.Price == null)
              .ToList();

          if (leadsNoPriceList.Count > 0)
          {
              var defaultContactLanguage = "";
              foreach (var contactLead in leadsNoPriceList)
              {
                  defaultContactLanguage = defaultContactLanguage != "" ? defaultContactLanguage : GetDefaultLanguage(contactLead.Contact.ContactLanguages!);
                  var courseApiPrice = await GetPriceFromCourseApi(contactLead, defaultContactLanguage, cancellationToken);

                  if (courseApiPrice != 0)
                  {
                      contactLead.Price = courseApiPrice;
                  }
              }
              await _context.SaveChangesAsync(cancellationToken);
          }*/

        if (contactLeadList.Count == 0)
        {
            throw new NotFoundException("Lead not found!");
        }

        var fav = await _context.ContactLeadProcesses
            .Where(p => p.ProcessId == request.ProcessId)
            .Select(p => p.ContactLeadId)
            .ToListAsync(cancellationToken);
   

        List<ContactLeadDto> contactLeads = new();

        var countryCodes = contactLeadList.Select(cl => cl.CountryCode);
        List<Country> clCountries =
            await _context.Country.Where(c => countryCodes.Contains(c.CountryCode))
                .Include(c => c.Currency)
                .ToListAsync(cancellationToken);

        var courseTypeBases = await _context.CourseTypeBases.ToListAsync(cancellationToken);
        foreach (var contactLead in contactLeadList)
        {
            var dto = _mapper.Map<ContactLeadDto>(contactLead);
            dto.IsFavourite = fav is null? false : fav.Contains(contactLead.Id);

            dto.Types = ContactLeadDto.GetContactLeadTypeList(dto.IdContactTypes);
            var contactPhone = await _context.ContactPhone
                .Where(cp => !cp.IsDeleted && cp.IsDefault == true && cp.ContactId == dto.ContactId)
                .Select(cp => new {cp.PhonePrefix, cp.Phone}).FirstOrDefaultAsync(cancellationToken);

            dto.Contact.Phone = $"{contactPhone?.PhonePrefix} {contactPhone?.Phone}".Trim();

            var currency =  await _context.Currency
                .Where(c => c.CurrencyCode == dto.Currency)
                .FirstOrDefaultAsync(cancellationToken);
            var currencySymbol = currency?.CurrencySymbol ?? clCountries.FirstOrDefault(c => c.CountryCode == contactLead.CountryCode)?.Currency.CurrencySymbol;

            dto.CurrencyCountryCode = await GetCurrencyCountryCode(dto, clCountries, cancellationToken);

            if (currencySymbol is not null)
            {
                dto.CurrencySymbol = currencySymbol;
            }

            var currencyDisplayFormat = currency?.CurrencyDisplayFormat ?? clCountries.FirstOrDefault(c => c.CountryCode == contactLead.CountryCode)?.Currency?.CurrencyDisplayFormat;

            if (currencyDisplayFormat is not null)
            {
                dto.CurrencyDisplayFormat = currencyDisplayFormat;
            }

            if (contactLead.CourseTypeBaseCode is not null)
            {
                var courseTypeBase = courseTypeBases.SingleOrDefault(c => c.Code == contactLead.CourseTypeBaseCode);
                if (courseTypeBase is not null)
                {
                    dto.CourseTypeBase =  _mapper.Map<CourseTypeBaseDto>(courseTypeBase);
                }
            }
            contactLeads.Add(dto);
        }

        return contactLeads;
    }

    //private async Task<List<ContactLeadDto>> GetContactLeadDtosAsync(
    //GetContactLeadsByContactAndProcessQuery request,
    //CancellationToken cancellationToken)
    //{
    //    return await _context.ContactLeads
    //        .Where(cl => !cl.IsDeleted && cl.ContactId == request.ContactId)
    //        .OrderBy(cl => cl.Id)
    //        .Take(10)
    //        .AsSplitQuery()
    //        .ProjectTo<ContactLeadDto>(_mapper.ConfigurationProvider)
    //        .ToListAsync(cancellationToken);
    //}

    private async Task<List<ContactLeadDto>> GetContactLeadDtosAsync(
    GetContactLeadsByContactAndProcessQuery request,
    CancellationToken cancellationToken)
    {
        var result = await _context.ContactLeads
            .Where(cl => !cl.IsDeleted && cl.ContactId == request.ContactId)
            .OrderBy(cl => cl.Id)
            .Take(10)
            .AsSplitQuery()
            .ProjectTo<ContactLeadDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // Procesar el campo Types manualmente
        result.ForEach(dto =>
        {
            dto.Types = ContactLeadDto.GetContactLeadTypeList(dto.IdContactTypes);
        });
            return result;
    }

    //private async Task<List<ContactLeadDto>> GetContactLeadDtosAsync(GetContactLeadsByContactAndProcessQuery request, CancellationToken cancellationToken)
    //{
    //    return await _context.ContactLeads
    //        .Where(cl => !cl.IsDeleted && cl.ContactId == request.ContactId)
    //        .Select(cl => new ContactLeadDto
    //        {
    //            Id = cl.Id,
    //            CountryCode = cl.CountryCode,
    //            UserCountryCode = cl.UserCountryCode,
    //            CurrencyCountryCode = cl.CurrencyCountryCode,
    //            AreaUrl = cl.AreaUrl,
    //            Title = cl.Title,
    //            CourseId = cl.CourseId,
    //            Course = cl.Course != null ? new CourseDto
    //            {
    //                Id = cl.Course.Id,
    //                OriginalCourseId = cl.Course.OriginalCourseId,
    //                CourseTypeId = cl.Course.CourseTypeId,
    //                CourseType = cl.Course.CourseType != null ? new CourseTypeDto
    //                {
    //                    Id = cl.Course.CourseType.Id,
    //                    Name = cl.Course.CourseType.Name,
    //                    Label = cl.Course.CourseType.Label
    //                } : null,
    //                Title = cl.Course.Title,
    //                Code = cl.Course.Code,
    //                Guarantor = cl.Course.Guarantor != null ? new GuarantorDto
    //                {
    //                    OriginalGuarantorId = cl.Course.Guarantor.OriginalGuarantorId,
    //                    Name = cl.Course.Guarantor.Name,
    //                    ShortName = cl.Course.Guarantor.ShortName,
    //                    LongName = cl.Course.Guarantor.LongName,
    //                    Label = cl.Course.Guarantor.Label
    //                } : null,
    //                CourseFaculties = cl.Course.CourseFaculties != null ? cl.Course.CourseFaculties.Select(cf => new CourseFacultyDto
    //                {
    //                    FacultyId = cf.FacultyId,
    //                    Faculty = cf.Faculty != null ? new FacultyDto
    //                    {
    //                        Id = cf.Faculty.Id,
    //                        Name = cf.Faculty.Name,
    //                        Label = cf.Faculty.Label,
    //                        SeoUrl = cf.Faculty.SeoUrl,
    //                        Color = cf.Faculty.Color,
    //                        Code = cf.Faculty.Code
    //                    } : null,
    //                    CourseId = cf.CourseId
    //                }).ToList() : null,
    //                CourseSpecialities = cl.Course.CourseSpecialities != null ? cl.Course.CourseSpecialities.Select(cs => new CourseSpecialityDto
    //                {
    //                    SpecialityId = cs.SpecialityId,
    //                    CourseId = cs.CourseId,
    //                    Speciality = cs.Speciality != null ? new SpecialityDto
    //                    {
    //                        Id = cs.Speciality.Id,
    //                        Name = cs.Speciality.Name,
    //                        Label = cs.Speciality.Name,
    //                        SeoUrl = cs.Speciality.SeoUrl,
    //                        SeoTitle = cs.Speciality.SeoTitle,
    //                        OriginalCategoryId = cs.Speciality.OriginalCategoryId.ToString(),
    //                        Code = cs.Speciality.Code
    //                    } : null,
    //                }).ToList() : null
    //            } : null,
    //            CourseDataId = cl.CourseDataId,
    //            CourseData = cl.CourseData != null ? new CourseDataDto
    //            {
    //                Id = cl.CourseData.Id,
    //                Title = cl.CourseData.Title,
    //                SeoTitle = cl.CourseData.SeoTitle
    //            } : null,
    //            OriginalCourseId = cl.OriginalCourseId,
    //            Enquiry = cl.Enquiry,
    //            ContactId = cl.ContactId,
    //            Contact = new ContactDto
    //            {
    //                Id = cl.Contact.Id,
    //                Name = cl.Contact.Name,
    //                FirstSurName = cl.Contact.FirstSurName,
    //                SecondSurName = cl.Contact.SecondSurName,
    //                StudentCIF = cl.Contact.StudentCIF,
    //                FiscalCIF = cl.Contact.FiscalCIF,
    //                Email = cl.Contact.Email,
    //                LegalName = cl.Contact.LegalName,
    //                IdCard = cl.Contact.IdCard,
    //                CountryCode = cl.Contact.CountryCode,
    //                ContactTypeId = cl.Contact.ContactTypeId,
    //                ContactStatusId = cl.Contact.ContactStatusId,
    //                Origin = cl.Contact.Origin,
    //                Profession = cl.Contact.Profession,
    //                ContactGenderId = cl.Contact.ContactGenderId,
    //                Nationality = cl.Contact.Nationality,
    //                OriginContactId = cl.Contact.OriginContactId,
    //                NextInteraction = cl.Contact.NextInteraction,
    //                KeyRegimeCode = cl.Contact.KeyRegimeCode,
    //                CustomerAccount = cl.Contact.CustomerAccount,
    //                Occupation = cl.Contact.Occupation,
    //                CenterName = cl.Contact.CenterName
    //            },
    //            Url = cl.Url,
    //            AccessUrl = cl.AccessUrl,
    //            Price = cl.Price,
    //            Currency = cl.Currency,
    //            Created = cl.Created,
    //            Faculty = cl.Faculty != null ? new FacultyDto
    //            {
    //                Id = cl.Faculty.Id,
    //                Name = cl.Faculty.Name,
    //                Label = cl.Faculty.Label,
    //                SeoUrl = cl.Faculty.SeoUrl,
    //                Color = cl.Faculty.Color,
    //                Code = cl.Faculty.Code
    //            } : null,
    //            IdContactTypes = cl.Types,
    //            CourseCountryId = cl.CourseCountryId.GetValueOrDefault(),
    //            FinalPrice = cl.FinalPrice,
    //            Discount = cl.Discount,
    //            EnrollmentPercentage = cl.EnrollmentPercentage,
    //            Fees = cl.Fees,
    //            CourseTypeName = cl.CourseTypeName,
    //            ContactTrackerDate = cl.ContactTrackerDate,
    //            ConvocationDate = cl.ConvocationDate,
    //            CourseTypeBaseCode = cl.CourseTypeBaseCode,
    //            StartDateCourse = cl.StartDateCourse,
    //            FinishDateCourse = cl.FinishDateCourse,
    //            LanguageId = cl.LanguageId,
    //            LanguageCode = cl.Language.Name,
    //            CouponOriginId = cl.CouponOriginId,
    //            CouponOrigin = cl.CouponOrigin != null ? new CouponsOriginsDto
    //            {
    //                Id = cl.CouponOrigin.Id,
    //                Name = cl.CouponOrigin.Name
    //            } : null,
    //            University = cl.University.Value == 0 ? University.TechUniversity : University.TechFP,
    //            CourseCode = cl.CourseCode
    //        })
    //        .OrderBy(cl => cl.Id)
    //        .Take(10)
    //        .AsSplitQuery()
    //        .ToListAsync(cancellationToken);
    //}

    private async Task<string> GetCurrencyCountryCode(ContactLeadDto contactLead, List<Country> countries, CancellationToken cancellationToken)
    {

        var country = countries.Find(c => c.Currency.CurrencyCode == contactLead.Currency && c.CountryCode == contactLead.CountryCode);

        if (country is not null)
        {
            return country.CountryCode;
        }

        country = countries.Find(c => c.Currency.CurrencyCode == contactLead.Currency);

        if (country is not null)
        {
            return country.CountryCode;
        }

        country = await _context.Country
            .Include(c => c.Currency)
            .Where(c => c.Currency.CurrencyCode == contactLead.Currency)
            .FirstOrDefaultAsync(cancellationToken);

        return country?.CountryCode ?? contactLead.CountryCode;
    }
    private async Task<decimal?> GetPriceFromCourseApi(ContactLead lead, string language, CancellationToken cancellationToken)
    {
        var paramsDto = new PricesByCodeCourseParamsDto(lead.CountryCode, language, true);

        if (lead.Course is not null)
        {
            paramsDto = paramsDto with
            {
                CourseCodes = lead.Course.Code,
                CountryCode = lead.CountryCode,
            };
        }

        try
        {
            var response = await _courseUnApiClient.GetMultiplePricesByCode(paramsDto, cancellationToken)
                .ConfigureAwait(false);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            JObject jsonObject = JObject.Parse(content);

            if (jsonObject?["data"]?["coursePrices"]?[0]?["price"] is null ||
                !decimal.TryParse(jsonObject["data"]["coursePrices"][0]["price"].ToString(), out var price))
            {
                return 0;
            }

            return price;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
    }

    private string GetDefaultLanguage(List<ContactLanguage> languages)
    {
        var defaultLanguage = _configuration["Constants:DefaultLanguageCode"];

        if (languages is null)
        {
            return defaultLanguage!;
        }

        if ( languages.Count > 0)
        {
            defaultLanguage = languages.FirstOrDefault(l => l.IsDefault)?.Language.Name;
        }

        return defaultLanguage!;

    }
}
