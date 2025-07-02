using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Emails.Queries.GetEmailDetails;

public class GetEmailDetailsQuery : IRequest<EmailFullDto>
{
    public int Id { get; set; }
}
    
public class GetEmailDetailsQueryHandler : IRequestHandler<GetEmailDetailsQuery, EmailFullDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
        
    public GetEmailDetailsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
        
    public async Task<EmailFullDto> Handle(GetEmailDetailsQuery request, CancellationToken cancellationToken)
    {
        var action = await _context.Emails
            .Include(e => e.Attachments)
            .Include(e => e.User)
            .Include(e => e.Action)
            .ThenInclude(a => a.Process)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (action == null)
        {
            throw new NotFoundException(nameof(Email), request.Id);
        }

        return _mapper.Map<EmailFullDto>(action);
    }
}
