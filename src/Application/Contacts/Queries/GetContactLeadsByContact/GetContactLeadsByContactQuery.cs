using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Queries.GetContactLeadsByContact;

public class GetContactLeadsByContactQuery : IRequest<List<ContactLeadDto>>
{
    public int ContactId { get; set; }
}
    
public class GetContactLeadsByContactQueryHandler : IRequestHandler<GetContactLeadsByContactQuery, List<ContactLeadDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetContactLeadsByContactQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
        
    public async Task<List<ContactLeadDto>> Handle(GetContactLeadsByContactQuery request, CancellationToken cancellationToken)
    {
        var set = await _context.ContactLeads
            .Where(cl => !cl.IsDeleted && cl.ContactId == request.ContactId)
            .Include(cl => cl.CourseType)
            .Include(cl => cl.CourseData)
            .Include(cl => cl.Contact)
            .ThenInclude(c => c.ContactAddress.Where(ca => ca.IsDefault == true))
            .ThenInclude(ca => ca.Country.Currency)
            .Include(cl => cl.Course)
            .ThenInclude(c => c.CourseType)
            .Include(cl => cl.Course.Guarantor)
            .Include(cl => cl.Course.CourseFaculties)
            .ThenInclude(cf => cf.Faculty)
            .Include(cl => cl.Course.CourseSpecialities)
            .ThenInclude(cs => cs.Speciality)
            .Include(cl => cl.Emails)
            .OrderByDescending(cl => cl.Id)
            .Take(10)
            .OrderBy(cl => cl.Id)
            .AsQueryable()
            .ToListAsync(cancellationToken);

        var contactProcesses = await _context.Processes
            .Where(p => p.ContactId == request.ContactId)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var fav = await _context.ContactLeadProcesses
            .Where(p => contactProcesses.Contains(p.ProcessId))
            .Select(p => p.ContactLeadId)
            .ToListAsync(cancellationToken);

        List<ContactLeadDto> contactLeads = new List<ContactLeadDto>();
            
        var countryCodes = set.Select(cl => cl.CountryCode);
        List<Country> clCountries =
            await _context.Country.Where(c => countryCodes.Contains(c.CountryCode))
                .Include(c => c.Currency)
                .ToListAsync(cancellationToken);

        foreach (var cl in set)
        {
            ContactLeadDto dto = _mapper.Map<ContactLeadDto>(cl);
            dto.IsFavourite = fav.Contains(cl.Id);
            dto.CurrencySymbol = clCountries.FirstOrDefault(c => c.CountryCode == cl.CountryCode)?.Currency.CurrencySymbol;
            dto.CurrencyDisplayFormat = clCountries.FirstOrDefault(c => c.CountryCode == cl.CountryCode)?.Currency
                .CurrencyDisplayFormat;
            contactLeads.Add(dto);
        }
        return contactLeads;
    }
}