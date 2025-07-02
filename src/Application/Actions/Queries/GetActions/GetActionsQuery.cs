using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Actions.Queries.GetActions;

public class GetActionsQuery : IRequest<List<ActionChildViewModel>>
{
    public int ProcessId { get; set; }
}
    
public class GetActionsQueryHandler: IRequestHandler<GetActionsQuery, List<ActionChildViewModel>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetActionsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ActionChildViewModel>> Handle(GetActionsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Actions
            .Include(a => a.Reassignment)
            .Include(a => a.User)
            .ProjectTo<ActionChildViewModel>(_mapper.ConfigurationProvider)
            .Where(a => a.ProcessId == request.ProcessId)
            .ToListAsync(cancellationToken);
    }
}