using System;
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
using Microsoft.Extensions.Configuration;
using ContactLeadDto = CroupierAPI.Contracts.Dtos.ContactLeadDto;
using ContactLeadType = CroupierAPI.Contracts.Enums.ContactLeadType;
using CourseData = IntranetMigrator.Domain.Entities.CourseData;

namespace CrmAPI.Application.ContactLeads.Commands.CreateOrUpdateContactLead;

public class CreateOrUpdateContactLeadCommand: ContactLeadDto, IRequest<int> { }

[UsedImplicitly]
public class CreateOrUpdateContactLeadHandler : IRequestHandler<CreateOrUpdateContactLeadCommand, int>
{
    private readonly string? _contactLeadRecommendedTypes;

    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPotentialsService _potentialsService;
    private readonly ICoursesUnDbContext _courseUnContext;
    private readonly IDateTime _dateTime;

    public CreateOrUpdateContactLeadHandler(
        IApplicationDbContext context,
        IPotentialsService potentialsService, IMapper mapper,
        ICoursesUnDbContext courseUnContext,
        IConfiguration configuration, IDateTime dateTime)
    {
        _context = context;
        _potentialsService = potentialsService;
        _courseUnContext = courseUnContext;
        _mapper = mapper;
        _dateTime = dateTime;

        _contactLeadRecommendedTypes = configuration["Constants:ContactLeadRecommendedType"];
    }
    public async Task<int> Handle(CreateOrUpdateContactLeadCommand request, CancellationToken cancellationToken)
    {
        if (request.CourseId == 0 && !string.IsNullOrEmpty(request.Enquiry)) {
            return await CreateContactLead(request, cancellationToken);
        }

        if (request.CourseId != 0)
        {
            var contactLead = await _context.ContactLeads
                .Where(cl => cl.ContactId == request.ContactId && cl.CourseId == request.CourseId && !cl.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (contactLead is null )
            {
                return await CreateContactLead(request, cancellationToken);
            }
            return await UpdateContactLead(contactLead.Id, request, cancellationToken);
        }

        return 0;
    }

    private async Task<int> CreateContactLead(CreateOrUpdateContactLeadCommand request, CancellationToken ct)
    {
        var contactLead = MapToDomain(request);

        // TODO: Optimize. Below is some code that can be simplified.
        if (contactLead.University == University.TechUniversity)
        {
            contactLead.CourseId = request.CourseId;
            contactLead.CourseDataId = request.CourseDataId;
        }
        else // TechFP
        {
            contactLead.CourseId = null;
            contactLead.CourseDataId = null;
        }

        if (request.CourseDataId <= 0)
        {
            contactLead.CourseDataId = contactLead.CourseDataId == 0 ? null : contactLead.CourseDataId;
            contactLead.CourseId = contactLead.CourseId == 0 ? null : contactLead.CourseId;
            _context.ContactLeads.Add(contactLead);
            await _context.SaveChangesAsync(ct);
            return 0;
        }

        var courseCountry = await _context.CourseCountries
            .FirstOrDefaultAsync(c => c.Id == request.CourseCountryId, ct);

        if (courseCountry == null)
        {
            throw new NotFoundException(nameof(CourseCountry), request.CourseCountryId);
        }

        await SyncCourseData(request.CourseDataId, ct);

        var language = await _context.Languages
            .FirstOrDefaultAsync(language => language.Name == request.LanguageCode, ct);

        contactLead.Currency = courseCountry?.CurrencyCode;

        if (contactLead.University == University.TechUniversity)
        {
            var faculty = await _context.Faculties.Where(f => f.Id == request.FacultyId)
                .FirstOrDefaultAsync(ct);

            if (faculty is not null)
            {
                contactLead.AreaUrl = faculty.SeoUrl ?? "";
                contactLead.FacultyId = faculty.Id;
                contactLead.Faculty = faculty;
            }
        }

        foreach (var type in request.Types)
        {
            if (type == ContactLeadType.Recommended)
            {
                continue;
            }

            var typeAsNumber = (int)(ContactLeadType)Enum.Parse(typeof(ContactLeadType), type.ToString());
            contactLead.Types = contactLead.Types + ',' + typeAsNumber;
        }

        _context.ContactLeads.Add(contactLead);
        await _context.SaveChangesAsync(ct);

        if (request.IsFavourite)
        {
            var newEntry = new ContactLeadProcess
            {
                IsDeleted = false,
                ContactLeadId = contactLead.Id,
                ProcessId = request.ProcessId,
            };
            _context.ContactLeadProcesses.Add(newEntry);
        }

        if (language is not null)
        {
            contactLead.LanguageId = language.Id;
        }

        if (request.StartDateCourse is not null)
        {
            contactLead.StartDateCourse = request.StartDateCourse;
        }

        if (request.FinishDateCourse is not null)
        {
            contactLead.FinishDateCourse = request.FinishDateCourse;
        }

        await _context.SaveChangesAsync(ct);

        var contactId = await _context.Contact
            .Where(c => c.Id == contactLead.ContactId)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(ct);

        if (contactId > 0)
        {
            await _potentialsService.CreateOrUpdateContactInPotentials(contactId, ct);
        }

        return contactLead.Id;
    }

    private ContactLead MapToDomain(CreateOrUpdateContactLeadCommand request) => new()
    {
        Types = _contactLeadRecommendedTypes,
        CourseTypeName = request.CourseTypeName ?? string.Empty,
        Url = request.Url,
        Price = request.Price,
        AreaUrl = request.AreaUrl,
        Enquiry = request.Enquiry,
        CourseId = request.CourseId,
        Currency = request.Currency,
        SentEmail = request.EmailSent,
        ContactId = request.ContactId,
        FacultyId = request.FacultyId,
        FinalPrice = request.FinalPrice,
        CountryCode = request.CountryCode,
        CourseDataId = request.CourseDataId,
        CouponOriginId = request.CouponOriginId,
        CourseCountryId = request.CourseCountryId,
        ConvocationDate = request.ConvocationDate,
        StartDateCourse = request.StartDateCourse,
        UserCountryCode = request.UserCountryCode,
        FinishDateCourse = request.FinishDateCourse,
        CourseTypeBaseCode = request.CourseTypeBaseCode,
        CurrencyCountryCode = request.CurrencyCountryCode,
        Fees = request.Fees.GetValueOrDefault(),
        Discount = request.Discount.GetValueOrDefault(),
        EnrollmentPercentage = request.EnrollmentPercentage.GetValueOrDefault(),
        University = Enum.TryParse<University>(request.University, out var value)
            ? value
            : University.TechUniversity,
    };

    private async Task SyncCourseData(int courseDataId, CancellationToken ct)
    {
        var oldCd = await _courseUnContext.CourseData
            .FirstOrDefaultAsync(cd => cd.Id == courseDataId, ct);

        if (oldCd is null)
        {
            return;
        }

        var cd = await _context.CourseData
            .FirstOrDefaultAsync(cd => cd.Id == courseDataId, ct);

        var dto = _mapper.Map<CourseDataSyncDto>(oldCd);
        var newEntity = _mapper.Map<CourseData>(dto);

        if (dto is null)
        {
            return;
        }

        var courseCountry =
            await _context.CourseCountries.FirstOrDefaultAsync(c => c.Id == newEntity.CourseCountryId, ct);

        if (courseCountry == null)
        {
            throw new NotFoundException($"CourseCountries not found! ({newEntity.CourseCountryId}) Need exec syns");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[CourseData] ON", ct);
            if (cd is not null)
            {
                _context.Entry(cd).CurrentValues.SetValues(newEntity);
            }
            else
            {
                _context.CourseData.Add(newEntity);
            }

            await _context.SaveChangesAsync(ct);
            await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[CourseData] OFF", ct);
            await transaction.CommitAsync(ct);
        }
        catch (Exception)
        {
#pragma warning disable CA2016
            // ReSharper disable once MethodSupportsCancellation
            await transaction.RollbackAsync();
#pragma warning restore CA2016

            throw;
        }
    }


    private async Task<int> UpdateContactLead(int contactLeadId, CreateOrUpdateContactLeadCommand request, CancellationToken cancellationToken)
    {
        var contactLead = await _context.ContactLeads
            .FirstOrDefaultAsync(cl => cl.Id == contactLeadId, cancellationToken);

        if (contactLead == null)
        {
            return 0;
        }

        contactLead.Discount = request.Discount ?? contactLead.Discount;
        contactLead.Price = contactLead.Price < 0 ? 0 : contactLead.Price;
        contactLead.FinalPrice = request.FinalPrice ?? contactLead.FinalPrice;
        contactLead.EnrollmentPercentage = request.EnrollmentPercentage ?? contactLead.EnrollmentPercentage;
        contactLead.Fees = request.Fees ?? contactLead.Fees;
        contactLead.CourseTypeBaseCode = request.CourseTypeBaseCode ?? contactLead.CourseTypeBaseCode;
        contactLead.StartDateCourse = request.StartDateCourse ?? contactLead.StartDateCourse;
        contactLead.FinishDateCourse = request.FinishDateCourse ?? contactLead.FinishDateCourse;
        contactLead.ConvocationDate = request.ConvocationDate ?? contactLead.ConvocationDate;
        contactLead.Enquiry = $"{contactLead.Enquiry} {request.Enquiry}".Trim() ;
        contactLead.ContactTrackerDate = DateTime.Now;

        if (request.CouponOriginId is not null)
        {
            contactLead.CouponOriginId = request.CouponOriginId;
        }

        if (request.Types != null && request.Types.Count > 0)
        {
            contactLead.Types = contactLead.Types + "," + string.Join(",", request.Types.Distinct().Select(t => ((int)t).ToString()));

            var tags = contactLead.Types.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            tags = tags.Select(tag => tag.Trim()).Distinct().ToList();
            contactLead.Types = string.Join(", ", tags);
        }

        return await _context.SaveChangesAsync(cancellationToken);
    }

}
