using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CroupierAPI.Contracts.Events;
using IntranetMigrator.Domain.Common;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CrmAPI.Application.Contacts.Commands.CreateContact;


public class CreateContactCommand: ContactCreateDto, IRequest<ContactCreated>
{
    public new bool? CreateProcess { get; set; } = true;
    public ProcessType? SelectedProcessType { get; set; }
}

[UsedImplicitly]
public class CreateContactCommandHandler : IRequestHandler<CreateContactCommand, ContactCreated>
{
    private readonly IMapper _mapper;
    private readonly IIsDefaultService _isDefault;
    private readonly IConfiguration _configuration;
    private readonly IApplicationDbContext _context;
    private readonly ICoursesUnDbContext _coursesUnContext;
    private readonly IProcessesService _processesService;
    private readonly IPotentialsService _potentialsService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IOrganizationNodeExplorerService _organizationNodeExplorerService;

    public CreateContactCommandHandler(IMapper mapper,
        IIsDefaultService isDefault,
        IConfiguration configuration,
        IApplicationDbContext context,
        ICoursesUnDbContext coursesUnContext,
        IProcessesService processesService,
        IPotentialsService potentialsService,
        ICurrentUserService currentUserService,
        IOrganizationNodeExplorerService organizationNodeExplorerService)
    {
        _context = context;
        _coursesUnContext = coursesUnContext;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _configuration = configuration;
        _isDefault = isDefault;
        _organizationNodeExplorerService = organizationNodeExplorerService;
        _potentialsService = potentialsService;
        _processesService = processesService;
    }
    public async Task<ContactCreated> Handle(CreateContactCommand request, CancellationToken ct)
    {
        // ESTABLECEMOS EL PAÍS POR DEFECTO
        SetDefaultContactCountry(request);

        // TODO: se ha quitado esto para ver si lo de los contactData hacen falta.
        // SINCROMIZAMOS LA COURSEDATA DE LOS CURSOS QUE NOS HAN LLEGADO
        // await SynchronizationCourseData(request, cancellationToken);

        // ESTABLEDEMOS LAS DIRECCIONES DEL CONTACTO
        await SetContactAddresses(request, ct);

        // NOS ASEGURAMOS DE ESTABLECER UN CORREO Y TELÉFONO POR DEFECTO
        await SetDefaultPhoneAndEmail(request);

        // GUARDAMOS LOS CURSOS
        await SetContactLeads(request, ct);

        // MAPEAMOS EL CONTACTO
        var contact = _mapper.Map<Contact>(request);

        // ESTABLECEMOS LAS FACULTADES Y ESPECIALIDADES DE ESTE CONTACTO
        // await SetContactFaculties(request, cancellationToken, contact);
        await SetContactSpecialities(request, ct, contact);

        // GUARDAMOS OTROS DATOS DEL CONTACTO
        await SetOthersContactData(request, ct, contact);

        // GUARDAMOS EL CONTACTO EN LA BASE DE DATOS
        _context.Contact.Add(contact);
        await _context.SaveChangesAsync(ct);

        // ACTUALIZAMOS LOS CAMBIOS EN LA BASE DE DATOS DE PONTENCIALES
        await _potentialsService.CreateOrUpdateContactInPotentials(contact.Id, ct, request.CouponOriginId);

        // SI HICIER FALTA, CREAMOS EL PROCESO PARA ESTE CONTACTO QUE ESTAMOS CREANDO
        var processId = await CreateProcessForNewContact(request, ct, contact);

        // DEVOLVEMOS LA INFORMACIÓN DEL CONTACTO Y PROCESO CREADOS
        return new ContactCreated
        {
            ContactId = contact.Id,
            ProcessId = processId,
            CorrelationId = contact.Guid
        };
    }

    private async Task<int> CreateProcessForNewContact(CreateContactCommand request, CancellationToken ct, Contact contact)
    {
        if (request.CreateProcess is not true)
        {
            return 0;
        }

        var newProcessType = request.SelectedProcessType == null ? ProcessType.Records2 : (ProcessType)request.SelectedProcessType!;
        return await CreateProcess(contact, newProcessType, request.University, ct);
    }

    private async Task SetContactSpecialities(CreateContactCommand request, CancellationToken ct, Contact contact)
    {
        var newSpecialityList = new List<Speciality>();
        var specialityList = await _context.Specialities.ToListAsync(ct);
        if (request.Specialities is not null && request.Specialities.Count > 0)
        {
            newSpecialityList.AddRange(request.Specialities
                .Select(speciality => specialityList.Find(f => f.Id == speciality.SpecialityId))
                .Where(newSpeciality => newSpeciality is not null)!);
            contact.Specialities = newSpecialityList;
        }
    }

    private async Task SetContactFaculties(CreateContactCommand request, CancellationToken ct,
        Contact contact)
    {
        var newFacultyList = new List<Faculty>();
        var facultyList = await _context.Faculties.ToListAsync(ct);

        if (request.Faculties is not null && request.Faculties.Count > 0)
        {
            newFacultyList.AddRange(request.Faculties.Select(faculty => facultyList.Find(f => f.Id == faculty.FacultyId))!);
            contact.Faculties = newFacultyList;
        }
    }

    private async Task SetOthersContactData(CreateContactCommand request, CancellationToken ct, Contact contact)
    {
        contact.University = request.University.ToString();
        contact.Origin = string.IsNullOrEmpty(contact.Origin)
            ? _configuration["Constants:ContactOriginManual"]
            : contact.Origin;

        contact.CurrencyId = await GetCurrency(request, ct);
    }

    private async Task SetDefaultPhoneAndEmail(CreateContactCommand request)
    {
        if (request.ContactPhone.Count > 0)
        {
            await _isDefault.SetIsDefault(request.ContactPhone);
        }

        if (request.ContactEmail.Count > 0)
        {
            await _isDefault.SetIsDefault(request.ContactEmail);
        }
    }

    private void SetDefaultContactCountry(CreateContactCommand request)
    {
        request.CountryCode ??= _configuration["Constants:SpainCountryCode"];
    }

    private async Task SynchronizationCourseData(ContactCreateDto request, CancellationToken ct)
    {
        if (request.ContactLeads is null && !request.ContactLeads!.Any())
        {
            return;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[CourseData] ON", ct);

            var courseDataIds = request.ContactLeads!.Select(cl => cl.CourseDataId).ToList();

            var coursesData = await _context.CourseData
                .Where(cd => courseDataIds.Contains(cd.Id))
                .Select(cd => cd.Id)
                .ToListAsync(ct);

            foreach (var cl in request.ContactLeads!)
            {
                if (cl.CourseDataId != null && !coursesData.Contains(cl.CourseDataId ?? 0))
                {
                    var cd = await _coursesUnContext.CourseData
                        .FirstOrDefaultAsync(courseData => courseData.Id == cl.CourseDataId, ct);
                    var dto = _mapper.Map<CourseDataSyncDto>(cd);
                    var newEntity = _mapper.Map<CourseData>(dto);
                    _context.CourseData.Add(newEntity);
                }
            }

            await _context.SaveChangesAsync(ct);
            await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[CourseData] OFF", ct);
            await _context.Database.CommitTransactionAsync(ct);
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

    private async Task<int> GetCurrency(CreateContactCommand request, CancellationToken ct)
    {
        var currency = await _context.Currency
            .Where(c => c.CurrencyCode == request.CurrencyCode)
            .FirstOrDefaultAsync(ct);

        return currency?.Id ?? 1;
    }

    private async Task<int> CreateProcess(BaseEntity contact, ProcessType processType, University university, CancellationToken ct)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .ThenInclude(e => e.CurrentOrganizationNode)
            .ThenInclude(o => o.Country)
            .FirstOrDefaultAsync(u => u.Employee.CorporateEmail == _currentUserService.Email,
                ct);

        if (user?.Employee.CurrentOrganizationNode == null)
        {
            return 0;
        }

        var newProcessType = ProcessType.Records2;
        var color = Colour.Grey;

        // If by default it is of type RECORDS, we discard that it is not of type visitors.
        if (processType == ProcessType.Records2)
        {
            var tagId = await _organizationNodeExplorerService
                .GetTagIdNode(user.Employee.CurrentOrganizationNode, _configuration["Constants:TagCrmFlows"]!);
            if (tagId != 0)
            {
                newProcessType = tagId == int.Parse(_configuration["Constants:CRMTagVisitors"]!)
                    ? ProcessType.Visits
                    : ProcessType.Records2;
                color = tagId == int.Parse(_configuration["Constants:CRMTagVisitors"]!) ? Colour.Green : Colour.Grey;
            }
        }
        else
        {
            newProcessType =  processType;
        }

        var process = new Process
        {
            ContactId = contact.Id,
            UserId = user.Id,
            Type = newProcessType,
            Status = ProcessStatus.ToDo,
            Outcome = ProcessOutcome.Open,
            ProcessOrigin = ProcessOrigin.Newcontact,
            Colour = color,
            University = university
        };

        return await _processesService.CreateProcess(process, ct);
    }

    private async Task SetContactAddresses(ContactCreateDto request, CancellationToken ct)
    {
        if (request.ContactAddress is { Count: > 0 })
        {
            await _isDefault.SetIsDefault(request.ContactAddress);
            foreach (var ca in request.ContactAddress)
            {
                var countryAux = await _context.Country
                     .FirstOrDefaultAsync(c => c.CountryCode == ca.CountryCode, ct)
                         ?? await _context.Country
                             .FirstOrDefaultAsync(c => c.CountryCode == _configuration["Constants:SpainCountryCode"], ct);
                ca.CountryId = countryAux?.Id;
            }
        }
        else
        {
            const int fiscalAddress = 1;
            var address = new ContactAddressCreateDto
            {
                CountryId = int.Parse(_configuration["Constants:SpainCountryId"]!),
                IsDefault = true,
                Address = "",
                City = "",
                Department = "",
                Province = "",
                PostalCode = "",
                AddressTypeId = fiscalAddress
            };
            request.ContactAddress = new List<ContactAddressCreateDto> { address };
        }
    }

    private async Task SetContactLeads(ContactCreateDto request, CancellationToken ct)
    {
        if (request.ContactLeads is { Count: > 0 })
        {
            foreach (var cl in request.ContactLeads)
            {
                var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == cl.CourseId, ct);
                if (course == null)
                {
                    request.ContactLeads = new List<ContactLeadCreateDto>();
                }

                var courseCountry = await _context.CourseCountries
                    .FirstOrDefaultAsync(c => c.Id == cl.CourseCountryId, ct);
                cl.Currency = courseCountry?.CurrencyCode;


                if (request.University == University.TechUniversity)
                {
                    var faculty = await _context.Faculties.Where(f => f.Name.Contains(cl.FacultyName))
                        .FirstOrDefaultAsync(ct);
                    cl.AreaUrl = cl.FacultyName ?? "";
                    cl.FacultyId = faculty?.Id;
                }

                if (request.University != University.TechUniversity)
                {
                    cl.CourseId = null;
                    cl.CourseDataId = null;
                }


                cl.FinalPrice = cl.Price;

                var language = await _context.Languages.Where(l => l.Name == cl.LanguageCode)
                    .FirstOrDefaultAsync(ct);

                if (language is not null)
                {
                    cl.LanguageId = language.Id;
                }
            }
        }
    }
}
