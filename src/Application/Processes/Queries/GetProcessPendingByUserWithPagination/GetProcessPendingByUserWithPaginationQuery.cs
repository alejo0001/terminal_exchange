using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Mappings;
using CrmAPI.Application.Common.Models;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Techtitute.DynamicFilter.Services;
using System;

namespace CrmAPI.Application.Processes.Queries.GetProcessPendingByUserWithPagination;

public class GetProcessPendingByUserWithPaginationQuery : IRequest<PaginatedList<ProcessDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string QueryParams { get; set; } = "";
    public string[]? OrderBy { get; set; } = { "Id" };
    public string[]? Order { get; set; } = { "asc" };
    public string? UserData { get; set; }
}

public class GetProcessPendingByUserWihtPaginationQueryHandler : IRequestHandler<GetProcessPendingByUserWithPaginationQuery, PaginatedList<ProcessDto>> 
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly IAuthService _authService;

    public GetProcessPendingByUserWihtPaginationQueryHandler(IApplicationDbContext context, IDateTime dateTime,
        ICurrentUserService currentUserService, IMapper mapper, IAuthService authService)
    {
        _context = context;
        _dateTime = dateTime;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _authService = authService;
    }
        
    public async Task<PaginatedList<ProcessDto>> Handle(GetProcessPendingByUserWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var user = await _authService.GetCurrentUserToRequest(request.UserData, "Administrador", cancellationToken);
        
        if (user is null)
        {
            throw new NotFoundException("User not found!");
        }

        // TODO: REVISAR Y DEJAR MAS LIMPIO
        var set = _context.Processes
            .Include(p => p.Contact)
            .Include(p => p.Actions)
            .Where(p => !p.IsDeleted 
                        && p.UserId == user.Id
                        && p.Status == ProcessStatus.Pending)
            .ApplyParameters(_context, request.QueryParams)
            .OrderByDescending(p => p.Actions.OrderByDescending(a => a.Date).FirstOrDefault().Date)
            .AsQueryable();
            
        if (String.Join(",", request.OrderBy!.ToArray()) != "Id")
        {
            set = set.OrderByDynamic(request.OrderBy, request.Order);
        }

        return await set.ProjectTo<ProcessDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}