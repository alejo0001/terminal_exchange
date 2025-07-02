using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using MediatR;

namespace CrmAPI.Application.Users.Queries.GetUsers;

public class GetUsersQuery : IRequest<List<UserDto>>
{
}
    
public class GetAppointmentsQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
        
    public GetAppointmentsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
        
    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        
        
        var set = _context.Users
            .Where(c => !c.IsDeleted)
            .AsQueryable();
        return await Task.Run(() => set.ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToList(), cancellationToken);
    }
}