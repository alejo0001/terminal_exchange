using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Whatsapps.Commands;

public class SendWhatsappCommand : WhatsappSendDto, IRequest<int>
{
}

public class SendWhatsappCommandHandler : IRequestHandler<SendWhatsappCommand, int>
{

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public SendWhatsappCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper
        )
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<int> Handle(SendWhatsappCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Employee.CorporateEmail.Equals(_currentUserService.Email) , cancellationToken);
        
        if (user == null)
            throw new ForbiddenAccessException();

        var whatsappSent = _mapper.Map<Whatsapp>(request);

        whatsappSent.UserId = user.Id;     
        
        _context.Whatsapps.Add(whatsappSent);
        await _context.SaveChangesAsync(cancellationToken);
        
        return whatsappSent.ContactId;
    }
}