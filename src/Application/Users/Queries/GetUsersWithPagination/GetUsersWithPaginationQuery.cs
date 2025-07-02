using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Mappings;
using CrmAPI.Application.Common.Models;
using MediatR;
using Techtitute.DynamicFilter.Services;

namespace CrmAPI.Application.Users.Queries.GetUsersWithPagination;

public class GetUsersWithPaginationQuery : IRequest<PaginatedList<UserDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string QueryParams { get; set; } = "";
    public string[] OrderBy { get; set; } = { "Id" };
    public string[] Order { get; set; } = { "asc" };
}

public class
    GetUsersWithPaginationQueryHandler : IRequestHandler<GetUsersWithPaginationQuery, PaginatedList<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUsersWithPaginationQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<UserDto>> Handle(GetUsersWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        var set = _context.Users
            .Where(a => !a.IsDeleted)
            .AsQueryable();
        if (!String.IsNullOrEmpty(request.QueryParams) && request.QueryParams != "\"\"")
        {
            set = set.ApplyParameters(_context, request.QueryParams);
        }

        set = set.OrderByDynamic(request.OrderBy, request.Order);

        return await set.ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}