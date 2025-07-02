using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Mappings;
using CrmAPI.Application.Common.Models;
using CrmAPI.Application.Common.Security;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Techtitute.DynamicFilter.Services;

namespace CrmAPI.Application.Processes.Queries.GetProcessesInProgressByUserWithPagination;

[Authorize(Roles = "Usuario")]
public class GetProcessesInProgressByUserWithPaginationQuery : IRequest<PaginatedList<ProcessInProgressDto>>
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
    
public class GetProcessesInProgressByUserWithPaginationQueryHandler : IRequestHandler<GetProcessesInProgressByUserWithPaginationQuery, PaginatedList<ProcessInProgressDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthService _authService;

    /// <summary>
    ///     This must be Expression tree, compatible with LINQ to EF, meaning that regular IF-s will explode!
    /// </summary>
    private static readonly Expression<Func<Process, ProcessInProgressDto>> Projection = (process) => new ProcessInProgressDto
    {
        ProcessId = process.Id,
        ContactId = process.ContactId,
        UserId = process.UserId,
        InitialDate = process.InitialDate,
        Created = process.Created,
        LastActionDate = process.Actions
            .OrderByDescending(a => a.Date)
            .Select(a => a.Date)
            .FirstOrDefault(),

        NextInteractionDate = process.Contact.NextInteraction ?? DateTime.Today,
        Attempts = process.Actions.Count(a => !a.IsDeleted && a.Type == ActionType.Call),
        Colour = process.Colour.ToString().ToLowerInvariant(),
        Type = process.Type.ToString().ToLowerInvariant(),
        Status = process.Status.ToString().ToLowerInvariant(),
        ActiveCall = process.Actions.Any(a => a.FinishDate == null && a.Type == ActionType.Call),

        ContactInProgressDto = process.Contact != null
            ? new ContactInProgressDto
            {
                Name = process.Contact.Name,
                FirstSurname = process.Contact.FirstSurName,
                SecondSurname = process.Contact.SecondSurName,
                IdCard = process.Contact.IdCard,
                Nationality = process.Contact.CountryCode,
                Email = process.Contact.ContactEmail
                    .Where(ce => !ce.IsDeleted)
                    .OrderByDescending(ce => ce.IsDefault)
                    .Select(ce => ce.Email)
                    .FirstOrDefault(),

                Phone = process.Contact.ContactPhone
                    .Where(cp => !cp.IsDeleted)
                    .OrderByDescending(cp => cp.IsDefault)
                    .Select(cp => cp.Phone)
                    .FirstOrDefault(),

                WorkCenter = process.Contact.WorkCenter,
                Faculties = process.Contact.Faculties
                    .Select(f => new FacultiesInProgressDto
                    {
                        Color = f.Color,
                        Name = f.Name,
                        SeoUrl = f.SeoUrl
                    }).ToList(),
                Specialities = process.Contact.Specialities
                    .Select(s => new SpecialitiesInProgressDto
                    {
                        Name = s.Name,
                        SeoUrl = s.SeoUrl
                    }).ToList(),
            }
            : null
    };

    public GetProcessesInProgressByUserWithPaginationQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IAuthService authService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _authService = authService;
    }

    public async Task<PaginatedList<ProcessInProgressDto>> Handle(
        GetProcessesInProgressByUserWithPaginationQuery request, CancellationToken ct)
    {
        var userData = request.UserId;

        if (!String.IsNullOrWhiteSpace(request.UserData))
        {
            var user = await _authService.GetCurrentUserToRequest(request.UserData, "Administrador", ct);
            userData = user.Id;
        }

        //TODO: refactorizar esta consulta zurda
        return await _context.Processes
            .Where(p => !p.IsDeleted && p.UserId == userData && p.Status != ProcessStatus.Closed &&
                        p.Status != ProcessStatus.Pending
                        && (request.CountryCode == "" || p.Contact.CountryCode == request.CountryCode)
                        && (string.IsNullOrEmpty(request.email) ||
                            p.Contact.ContactEmail.Any(e => e.Email.Contains(request.email) && !e.IsDeleted))
                        && (string.IsNullOrEmpty(request.phone) ||
                            p.Contact.ContactPhone.Any(p => p.Phone.Contains(request.phone) && !p.IsDeleted))
                        && (request.FacultiesList.Count == 0 && request.SpecialitiesList.Count == 0
                            || request.FacultiesList.Count == 0 &&
                            p.Contact.Specialities.Any(f => request.SpecialitiesList.Contains(f.Id))
                            || request.SpecialitiesList.Count == 0 &&
                            p.Contact.Faculties.Any(f => request.FacultiesList.Contains(f.Id))
                            || p.Contact.Specialities.Any(f => request.SpecialitiesList.Contains(f.Id))
                            && p.Contact.Faculties.Any(f => request.FacultiesList.Contains(f.Id))))
            .Where(p => !((p.Type == ProcessType.Records2 || p.Type == ProcessType.Activations) &&
                          p.Created.Date == DateTime.Today))
            .ApplyParameters(_context, request.QueryParams)
            .OrderByDescending(p => p.Type == ProcessType.NonPayment)
            .ThenBy(p => p.Contact.NextInteraction ?? DateTime.Now)
            .Select(Projection)
            .AsNoTracking()
            .AsSplitQuery()
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}