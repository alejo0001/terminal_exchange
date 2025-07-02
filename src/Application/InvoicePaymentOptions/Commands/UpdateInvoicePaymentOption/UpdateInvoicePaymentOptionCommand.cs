using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.InvoicePaymentOptions.Commands.UpdateInvoicePaymentOption;

public class UpdateInvoicePaymentOptionCommand: InvoicePaymentOptionUpdateDto, IRequest
{
}
    
public class UpdateInvoicePaymentOptionCommandHandler : IRequestHandler<UpdateInvoicePaymentOptionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateInvoicePaymentOptionCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
        
    public async Task<Unit> Handle(UpdateInvoicePaymentOptionCommand request, CancellationToken cancellationToken)
    {
        InvoicePaymentOption optionAux = _mapper.Map<InvoicePaymentOption>(request);
        InvoicePaymentOption option = _context.InvoicePaymentOptions
            .Include(c => c.Contacts)
            .FirstOrDefault(c => c.Id == request.Id);
        if (option == null)
        {
            throw new NotFoundException(nameof(InvoicePaymentOption), request.Id);
        }

        PropertyInfo[] properties = option.GetType().GetProperties();
        foreach (PropertyInfo property in properties)
        {
            var value = property.GetValue(optionAux, null);
            if (value != null && !property.Name.Equals("Id") 
                              && !property.Name.Contains("Created")
                              && !property.Name.Equals("Contacts"))
            {
                option.GetType().GetProperty(property.Name)?.SetValue(option, value, null);
            }
        }

        if (optionAux.Contacts.Count > 0)
        {
            if (option.Contacts != null && option.Contacts.Count > 0)
            {
                List<ContactInvoicePaymentOption> options = await _context.ContactInvoicePaymentOption.Where(c => c.InvoicePaymentOptionId == request.Id).ToListAsync(cancellationToken);
                _context.ContactInvoicePaymentOption.RemoveRange(options);
                await _context.SaveChangesAsync(cancellationToken);
            }
            foreach (Contact cc in optionAux.Contacts)
            {
                _context.ContactInvoicePaymentOption.Add(new ContactInvoicePaymentOption() { ContactId = cc.Id, InvoicePaymentOptionId = request.Id });
            }
            await _context.SaveChangesAsync(cancellationToken);
        }
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}