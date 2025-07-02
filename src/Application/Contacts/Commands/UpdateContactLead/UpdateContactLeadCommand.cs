using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Contacts.Commands.UpdateContactLead;

public class UpdateContactLeadCommand: ContactLeadUpdateDto, IRequest<int>
{
}

public class UpdateContactCommandHandler : IRequestHandler<UpdateContactLeadCommand, int>
{
    private readonly IApplicationDbContext _context;

    public UpdateContactCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(UpdateContactLeadCommand request, CancellationToken cancellationToken)
    {
        var contactLead = await _context.ContactLeads
            .FirstOrDefaultAsync(cl => cl.Id == request.ContactLeadId, cancellationToken);
            
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
        contactLead.CourseCode = request.CourseCode ?? contactLead.CourseCode;

        if (request.Types != null && request.Types.Count > 0)
        {
            contactLead.Types = string.Join(",", request.Types.Distinct().Select(t => ((int)t).ToString()));
        }
        return await _context.SaveChangesAsync(cancellationToken);
    }
}