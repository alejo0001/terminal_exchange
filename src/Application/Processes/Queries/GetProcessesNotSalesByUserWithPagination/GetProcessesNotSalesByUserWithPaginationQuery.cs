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

namespace CrmAPI.Application.Processes.Queries.GetProcessesNotSalesByUserWithPagination;

public class GetProcessesNotSalesByUserWithPaginationQuery : IRequest<PaginatedList<ProcessDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string QueryParams { get; set; } = "";
    public string[]? OrderBy { get; set; } = { "Id" };
    public string[]? Order { get; set; } = { "asc" };
}


public class GetProcessesNotSalesByUserWithPaginationQueryHandler : IRequestHandler<GetProcessesNotSalesByUserWithPaginationQuery, PaginatedList<ProcessDto>> 
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetProcessesNotSalesByUserWithPaginationQueryHandler(IApplicationDbContext context, IDateTime dateTime,
        ICurrentUserService currentUserService, IMapper mapper)
    {
        _context = context;
        _dateTime = dateTime;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }
        
    public async Task<PaginatedList<ProcessDto>> Handle(GetProcessesNotSalesByUserWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Employee.CorporateEmail == _currentUserService.Email, 
                cancellationToken);
        
        if (user is null)
        {
            throw new NotFoundException("User not found!");
        }

        var set = _context.Processes
            .Include(p => p.Contact)
            .Include(p => p.Actions)
            .Where(p => !p.IsDeleted 
                        && p.UserId == user.Id
                        && p.Outcome != ProcessOutcome.Sale
                        && p.Outcome != ProcessOutcome.PaymentMethodPending
                        && p.Status == ProcessStatus.Closed &&
                        p.Actions.Any(a => a.Date.Date >= _dateTime.Now.AddDays(-45)))
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

