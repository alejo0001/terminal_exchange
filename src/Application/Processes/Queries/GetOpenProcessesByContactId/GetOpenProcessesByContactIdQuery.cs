using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Processes.Queries.GetOpenProcessesByContactId;

public class GetOpenProcessesByContactIdQuery: IRequest<List<ProcessDto>>
{
    public int ContactId { get; set; }
}

public class GetOpenProcessesByContactIdQueryHandler : IRequestHandler<GetOpenProcessesByContactIdQuery, List<ProcessDto>>
{

    private readonly IApplicationDbContext _context;

    private readonly IMapper _mapper;
    public GetOpenProcessesByContactIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ProcessDto>> Handle(GetOpenProcessesByContactIdQuery request, CancellationToken cancellationToken)
    {
        return  await _context.Processes
            .Where(p => p.ContactId == request.ContactId && !p.IsDeleted)
            .Where(p => p.Status == ProcessStatus.ToDo || p.Status == ProcessStatus.Ongoing)
            .ProjectTo<ProcessDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}