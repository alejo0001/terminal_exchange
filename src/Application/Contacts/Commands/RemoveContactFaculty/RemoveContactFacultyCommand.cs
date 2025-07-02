using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Commands.RemoveContactFaculty;

public class RemoveContactFacultyCommand : IRequest
{
    public int FacultyId { get; set; }
    public int ContactId { get; set; }
}
    
public class RemoveContactFacultyCommandHandler : IRequestHandler<RemoveContactFacultyCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IPotentialsService _potentialsService;

    public RemoveContactFacultyCommandHandler(IApplicationDbContext context, IPotentialsService potentialsService)
    {
        _context = context;
        _potentialsService = potentialsService;
    }
        
    public async Task<Unit> Handle(RemoveContactFacultyCommand request, CancellationToken cancellationToken)
    {
        Contact contact = await _context.Contact
            .Include(c => c.Faculties)
            .Include(c => c.Specialities)
            .FirstOrDefaultAsync(c => c.Id == request.ContactId, cancellationToken);
        
        if (contact == null)
        {
            throw new NotFoundException("Contact not found!");
        }

        var facultyToRemove = contact.Faculties.FirstOrDefault(f => f.Id == request.FacultyId);
        contact.Faculties.Remove(facultyToRemove);

        List<Speciality> specialitiesList = (from s in _context.Specialities
                join cas in _context.CountryFacultySpecialities on s.Id equals cas.SpecialityId
                where cas.FacultyId == request.FacultyId
                select s)
            .ToList();
        //.ForEach(s => contact.Specialities.Remove(s));

        List<Speciality> duplicated = new List<Speciality>();
        foreach (var faculties in contact.Faculties)
        {
            List<Speciality> specialitiesList2 = (from s in _context.Specialities
                    join cas in _context.CountryFacultySpecialities on s.Id equals cas.SpecialityId
                    where cas.FacultyId == faculties.Id
                    select s)
                .ToList();

            foreach (var item in specialitiesList)
            {
                if (specialitiesList2.Contains(item)) duplicated.Add(item);
            }
        }

        foreach (var item in specialitiesList)
        {
            if(!duplicated.Contains(item)) contact.Specialities.Remove(item);
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _potentialsService.CreateOrUpdateContactInPotentials(contact.Id, cancellationToken);
        
        
        return Unit.Value;
    }
}