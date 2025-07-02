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
using CourseData = IntranetMigrator.Domain.Entities.CourseData;

namespace CrmAPI.Application.Contacts.Commands.CreateContactLead;

public class CreateContactLeadCommand: ContactLeadCreateDto, IRequest<int> { }

//[UsedImplicitly]
public class CreateContactLeadCommandHandler : IRequestHandler<CreateContactLeadCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICoursesUnDbContext _courseUnContext;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly IDateTime _dateTime;
    private readonly IPotentialsService _potentialsService;

    public CreateContactLeadCommandHandler(IApplicationDbContext context, ICoursesUnDbContext courseUnContext,
        IMapper mapper, IConfiguration configuration, IDateTime dateTime, IPotentialsService potentialsService)
    {
        _context = context;
        _courseUnContext = courseUnContext;
        _mapper = mapper;
        _configuration = configuration;
        _dateTime = dateTime;
        _potentialsService = potentialsService;
    }

    public async Task<int> Handle(CreateContactLeadCommand request, CancellationToken cancellationToken) // Return ContactLeadDto
    {

        CourseCountry? courseCountry = await _context.CourseCountries
            .FirstOrDefaultAsync(c => c.Id == request.CourseCountryId, cancellationToken);

        if (courseCountry == null)
        {
            throw new NotFoundException(nameof(CourseCountry), request.CourseCountryId);
        }

        if (request.University == University.TechUniversity && request.CourseDataId != null)
        {
            await SyncCourseData(request.CourseDataId ?? 0, cancellationToken);
        }


        var language = await _context.Languages
            .FirstOrDefaultAsync(language => language.Name == request.LanguageCode, cancellationToken);

        if(courseCountry.CurrencyCode != request.Currency)
        {
            throw new NotFoundException($"The currency: '{request.Currency}' is not associated with the country: {courseCountry.Name} ");
        }

        ContactLead contactLead = _mapper.Map<ContactLead>(request);
        contactLead.Types = _configuration["Constants:ContactLeadRecommendedType"];
        contactLead.ContactId = request.ContactId;
        contactLead.CourseId = request.University == University.TechUniversity ? request.CourseId : null;
        contactLead.CourseDataId = request.University == University.TechUniversity ? request.CourseDataId : null;
        contactLead.CountryCode = request.CountryCode;
        contactLead.Currency = request.Currency ?? courseCountry?.CurrencyCode;
        contactLead.Price = request.Price;
        contactLead.FinalPrice = request.FinalPrice;
        contactLead.Discount = request.Discount ?? 0;
        contactLead.EnrollmentPercentage = request.EnrollmentPercentage ?? 0;
        contactLead.Fees = request.Fees ?? 0;
        contactLead.AreaUrl = request.FacultyName ?? "";
        contactLead.Url = request.Url;
        contactLead.CourseTypeName = request.CourseTypeName ?? "";
        contactLead.CourseCountryId = request.CourseCountryId;
        contactLead.Discount = request.Discount ?? 0;
        contactLead.SentEmail = request.EmailSent;
        contactLead.SentMessage = request.MessageSent;
        contactLead.ConvocationDate = request.ConvocationDate ?? null;
        contactLead.CourseTypeBaseCode = request.CourseTypeBaseCode;
        contactLead.University = request.University;
        contactLead.Title = request.Title;


        if (request.University == University.TechUniversity)
        {
            Faculty? faculty = await _context.Faculties.Where(f => f.Name.Contains(request.FacultyName))
                .FirstOrDefaultAsync(cancellationToken);
            contactLead.FacultyId = faculty?.Id;
        }


        foreach (var type in request.Types)
        {
            if (type == ContactLeadType.Recommended)
            {
                continue;
            }
            var typeAsNumber = (int) ((ContactLeadType) Enum.Parse(typeof(ContactLeadType), type.ToString()));
            contactLead.Types = contactLead.Types + ',' + typeAsNumber;
        }

        _context.ContactLeads.Add(contactLead);
        await _context.SaveChangesAsync(cancellationToken);

        if (request.IsFavourite)
        {
            ContactLeadProcess newEntry = new ContactLeadProcess
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

        contactLead.Created = request.Created ?? _dateTime.Now;
        await _context.SaveChangesAsync(cancellationToken);

        var contactId = await _context.Contact
            .Where(c => c.Id == contactLead.ContactId)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (contactId > 0)
        {
            await _potentialsService.CreateOrUpdateContactInPotentials(contactId, cancellationToken);
        }

        return contactLead.Id;
    }

    private async Task SyncCourseData(int courseDataId, CancellationToken ct)
    {
        var oldCd = await _courseUnContext.CourseData
            .FirstOrDefaultAsync(cd => cd.Id == courseDataId, ct);

        var cd = await _context.CourseData
            .FirstOrDefaultAsync(cd => cd.Id == courseDataId, ct);
        var dto = _mapper.Map<CourseDataSyncDto>(oldCd);
        var newEntity = _mapper.Map<CourseData>(dto);

        if (newEntity is null)
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

            if (cd is { })
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
            // ReSharper disable once MethodSupportsCancellation
#pragma warning disable CA2016
            await transaction.RollbackAsync();
#pragma warning restore CA2016

            throw;
        }
    }
}
