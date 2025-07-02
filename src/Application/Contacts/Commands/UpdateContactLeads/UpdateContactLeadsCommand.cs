using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Commands.UpdateContactLeads;

public class UpdateContactLeadsCommand : IRequest
{
    public List<ContactLeadUpdateDto> ContactLeads { get; set; }
}

public class UpdateContactLeadsCommandHandler : IRequestHandler<UpdateContactLeadsCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateContactLeadsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateContactLeadsCommand request, CancellationToken cancellationToken)
    {
        var contactLeadsIds = request.ContactLeads.Select(cl => cl.ContactLeadId);

        var contactLeads = await _context.ContactLeads
            .Where(cl => contactLeadsIds.Contains(cl.Id))
            .ToListAsync(cancellationToken);


        foreach (var contactLeadBd in contactLeads)
        {
            var contactLead = request.ContactLeads
                .FirstOrDefault(cl => cl.ContactLeadId == contactLeadBd.Id);
            
            contactLeadBd.Discount = contactLead!.Discount ?? contactLeadBd.Discount;
            contactLeadBd.Price = contactLeadBd.Price < 0 ? 0 : contactLeadBd.Price;
            contactLeadBd.FinalPrice = contactLead.FinalPrice ?? contactLeadBd.FinalPrice;
            contactLeadBd.EnrollmentPercentage = contactLead.EnrollmentPercentage ?? contactLeadBd.EnrollmentPercentage;
            contactLeadBd.Fees = contactLead.Fees ?? contactLeadBd.Fees;
            contactLeadBd.CourseTypeBaseCode = contactLead.CourseTypeBaseCode ?? contactLeadBd.CourseTypeBaseCode;
            contactLeadBd.StartDateCourse = contactLead.StartDateCourse ?? contactLeadBd.StartDateCourse;
            contactLeadBd.FinishDateCourse = contactLead.FinishDateCourse ?? contactLeadBd.FinishDateCourse;
            contactLeadBd.ConvocationDate = contactLead.ConvocationDate ?? contactLeadBd.ConvocationDate;
            
            if (contactLead.Types != null && contactLead.Types.Count > 0)
            {
                contactLeadBd.Types = string.Join(",", contactLead.Types.Distinct().Select(t => ((int)t).ToString()));
            }
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}