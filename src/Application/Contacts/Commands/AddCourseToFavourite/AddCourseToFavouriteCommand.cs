using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Commands.AddCourseToFavourite;

public class AddCourseToFavouriteCommand: IRequest<int>
{
    public int ContactLeadId { get; set; }
    public int ProcessId { get; set; }
    public bool CourseFavourite { get; set; }
}

public class AddCourseToFavouriteCommandHandler : IRequestHandler<AddCourseToFavouriteCommand, int>
{
    private readonly IApplicationDbContext _context;

    public AddCourseToFavouriteCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(AddCourseToFavouriteCommand request, CancellationToken cancellationToken)
    {
        if (request.CourseFavourite)
        {
            ContactLeadProcess newEntry = new ContactLeadProcess
            {
                IsDeleted = false,
                ContactLeadId = request.ContactLeadId,
                ProcessId = request.ProcessId,
            };
            _context.ContactLeadProcesses.Add(newEntry);
        }
        else
        {
            ContactLeadProcess set = await _context.ContactLeadProcesses
                .Where(c => c.ContactLeadId == request.ContactLeadId && c.ProcessId == request.ProcessId)
                .FirstOrDefaultAsync();
            if (set != null) _context.ContactLeadProcesses.Remove(set);
        }

        var result = await _context.SaveChangesAsync(cancellationToken);
        return result;
    }
}