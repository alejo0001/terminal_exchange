using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Mappings;
using CrmAPI.Application.Common.Models;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Techtitute.DynamicFilter.Services;

namespace CrmAPI.Application.Processes.Queries.GetProcessesInProgressByUserWithPagination;

public class GetProcessesInProgressByUserWithPaginationQueryPocho : IRequest<PaginatedList<ProcessDto>>
{
    public int PageNumber { get; set; } = 1;
    
    public int UserId { get; set; }
    public int PageSize { get; set; } = 25;
    public string QueryParams { get; set; } = "";
    public string? email { get; set; } = "";
    public string? phone { get; set; } = "";
    public string? CountryCode { get; set; } = "";
    public string[]? OrderBy { get; set; } = { "Id" };
    public string[]? Order { get; set; } = { "asc" };
    public List<int>? FacultiesList { get; set; } = new List<int>();
    public List<int>? SpecialitiesList { get; set; } = new List<int>();
    public string? UserData { get; set; }
}
    
public class GetProcessesInProgressByUserWithPaginationQueryPochoHandler : IRequestHandler<GetProcessesInProgressByUserWithPaginationQueryPocho, PaginatedList<ProcessDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly IAuthService _authService;
        
    public GetProcessesInProgressByUserWithPaginationQueryPochoHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper,
        IAuthService authService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _authService = authService;
    }
        
    public async Task<PaginatedList<ProcessDto>> Handle(GetProcessesInProgressByUserWithPaginationQueryPocho request, CancellationToken cancellationToken)
    {
        var userData = request.UserId;

        if (!String.IsNullOrWhiteSpace(request.UserData))
        {
            var user = await _authService.GetCurrentUserToRequest(request.UserData, "Administrador", cancellationToken);
            userData = user.Id;
        }
        
        //TODO: refactorizar esta consulta zurda
        var set = _context.Processes
            .Include(p => p.Contact)
            .Where(p => !p.IsDeleted && p.UserId == userData && p.Status != ProcessStatus.Closed  && p.Status != ProcessStatus.Pending
                        && (request.CountryCode == "" || p.Contact.CountryCode == request.CountryCode)
                        && (string.IsNullOrEmpty(request.email) || p.Contact.ContactEmail.Any(e => e.Email.Contains(request.email) && !e.IsDeleted))
                        && (string.IsNullOrEmpty(request.phone)  || p.Contact.ContactPhone.Any(p => p.Phone.Contains(request.phone) && !p.IsDeleted))
                        && (request.FacultiesList.Count == 0 && request.SpecialitiesList.Count == 0
                            || request.FacultiesList.Count == 0 && p.Contact.Specialities.Any(f => request.SpecialitiesList.Contains(f.Id))
                            || request.SpecialitiesList.Count == 0 && p.Contact.Faculties.Any(f => request.FacultiesList.Contains(f.Id))
                            || p.Contact.Specialities.Any(f => request.SpecialitiesList.Contains(f.Id))
                            && p.Contact.Faculties.Any(f => request.FacultiesList.Contains(f.Id))))
            .Where(p=>!((p.Type == ProcessType.Records2 || p.Type == ProcessType.Activations) && p.Created.Date == DateTime.Today))
                       
            .ApplyParameters(_context, request.QueryParams)
            .OrderByDescending(p => p.Type == ProcessType.NonPayment)
            .ThenBy(p => p.Contact.NextInteraction ?? DateTime.Now)
            .AsSplitQuery();
        
            /* .ThenBy(p =>
                p.Colour == Colour.Green ? p.Contact.NextInteraction ?? DateTime.Now : DateTime.MaxValue)
            .ThenByDescending(p => p.Colour != Colour.Green ? p.Created : DateTime.MinValue)
            .AsSplitQuery(); */

        return await set.ProjectTo<ProcessDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}