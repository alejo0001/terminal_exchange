using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Emails.Queries.GetEmailsByContactId;

[Authorize]
public class GetEmailByContactIdAndProcessId : IRequest<List<EmailFullDto>>
{
    public int ContactId { get; set; }
    public int ProcessId { get; set; }
}

public class GetEmailByContactIdAndProcessIdQueryHandler: IRequestHandler<GetEmailByContactIdAndProcessId,List<EmailFullDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    public GetEmailByContactIdAndProcessIdQueryHandler(IMapper mapper, IApplicationDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }
    
    public async Task<List<EmailFullDto>> Handle(GetEmailByContactIdAndProcessId request, CancellationToken cancellationToken)
    {
        var emailsQry = await _context.Emails.Where(
            e => e.ContactId == request.ContactId &&
                 e.ProcessId == request.ProcessId).ToListAsync(cancellationToken);
        return _mapper.Map<List<EmailFullDto>>(emailsQry);
    }
}
