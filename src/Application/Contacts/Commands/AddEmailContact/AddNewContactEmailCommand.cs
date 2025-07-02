using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;

namespace CrmAPI.Application.Contacts.Commands.AddEmailContact;

public class AddNewContactEmailCommand : IRequest<int>
{
    public int ContactId { get; set; }
    public string Email { get; set; }
    public int EmailTypeId { get; set; }
    public bool IsDefault { get; set; }

}

public class AddNewContactEmailCommandHandler : IRequestHandler<AddNewContactEmailCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;
    private readonly IPotentialsService _potentialsService;
    public AddNewContactEmailCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, 
        IDateTime dateTime, IPotentialsService potentialsService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _potentialsService = potentialsService;
    }

    public async Task<int> Handle(AddNewContactEmailCommand request, CancellationToken cancellationToken)
    {
        var contact = _context.Contact.FirstOrDefault(i => i.Id == request.ContactId);

        if (contact == null)
        {
            throw new NotFoundException("Contact not found!");
        }

        var emails = _context.ContactEmail.Where(c => c.ContactId == contact.Id);
        var emailExist = emails.FirstOrDefault(e => e.Email == request.Email && !e.IsDeleted);
        if (emailExist != null)
        {
            throw new NotFoundException("email was added earlier");
        }
            
        var newEmail = new ContactEmail()
        {
            EmailTypeId = request.EmailTypeId,
            ContactId = contact.Id,
            Email = request.Email,
            Created = _dateTime.Now,
            CreatedBy = _currentUserService.Email,
            IsDefault = request.IsDefault
        };
        if (request.IsDefault)
        {
            foreach (var email in emails)
            {
                email.IsDefault = false;
            }
        }
        _context.ContactEmail.Add(newEmail);
        await _context.SaveChangesAsync(cancellationToken);
        await _potentialsService.CreateOrUpdateContactInPotentials(contact.Id, cancellationToken);
        
        return newEmail.Id;
    }
}