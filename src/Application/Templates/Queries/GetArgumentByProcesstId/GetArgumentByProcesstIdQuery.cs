using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CrmAPI.Application.Templates.Queries.GetArgumentByProcesstId;

public class GetArgumentByProcesstIdQuery : IRequest<TemplateDetailsDto>
{
    public int? ProcessId { get; set; }
    public int? ArgumentId { get; set; }
}

public class GetArgumentByProcesstIdQueryHandler : IRequestHandler<GetArgumentByProcesstIdQuery, TemplateDetailsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IProcessesService _processesService;
    private readonly IDateTime _dateTime;
    private readonly IMapper _mapper;
    private readonly  IOrganizationNodeExplorerService _organizationNodeExplorerService;
    private readonly ICurrentUserService _currentUserService;
    public GetArgumentByProcesstIdQueryHandler(IApplicationDbContext context, IProcessesService processesService, IDateTime dateTime, IMapper mapper, IConfiguration configuration, IOrganizationNodeExplorerService organizationNodeExplorerService, ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _processesService = processesService;
        _dateTime = dateTime;
        _configuration = configuration;
        _organizationNodeExplorerService = organizationNodeExplorerService;
        _currentUserService = currentUserService;
    }

    public async Task<TemplateDetailsDto?> Handle(GetArgumentByProcesstIdQuery request, CancellationToken ct)
    {
        var templateArgument = new TemplateDetailsDto();

        if (request.ArgumentId != null)
        {
            var argument = await _context.Templates
                .Where(tp => tp.Id ==  request.ArgumentId)
                .ProjectTo<TemplateDetailsDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(ct);

            if (argument != null)
            {
                templateArgument = argument;
            }
        }
        else
        {
            var process = await _context.Processes
            .Include(p => p.Contact)
                .ThenInclude(c => c.ContactLanguages.Where(cl => !cl.IsDeleted))
                .ThenInclude(cl => cl.Language)
            .Include(p => p.Contact)
                .ThenInclude(c => c.ContactLeads.Where(cl => !cl.IsDeleted))
            .Include(p => p.Actions)
            .Include(p => p.User)
                .ThenInclude(u => u.Employee)
                .ThenInclude(e => e.CurrentOrganizationNode)
            .FirstOrDefaultAsync(p => p.Id == request.ProcessId, ct);
        
            if (process == null)
            {
                throw new NotFoundException("Process not found!");
            }

            var processSettings = await GetProcessSettings(process, ct);

            // Encontramos el TagId del nodo más próximo en el que se encuentra el usuario, para enviarle las plantillas del
            // equipo al que pertenece
            var currentOrganizationNode = process!.User.Employee.CurrentOrganizationNode;

            var tagId = 12;
            var processType = process.Type;
            var templateType = TemplateType.Argument;
            var crmModuleId = Int32.Parse(_configuration["Constants:CRMModuleId"]!);
            var courseKnown = process.Contact.ContactLeads.Count > 0;
            var languageCode = await GetValidLanguageCode(process, process.User, ct);
            //var colour = GetValidColour(process.Colour!.Value);
            var day = await GetValidDay(process, processSettings);
            var attempt = GetValidAttempt(process, processSettings);
            
            var argument = await (
                    from tp in _context.TemplateProposals
                    join tpt in _context.TemplateProposalTemplates on tp.Id equals tpt.TemplateProposalId
                    join t in _context.Templates on tpt.TemplateId equals t.Id
                    join l in _context.Languages on t.LanguageId equals l.Id
                    where !tp.IsDeleted
                          && !tpt.IsDeleted
                          && !t.IsDeleted
                          && tp.ProcessType == processType
                          && tp.Day == day
                          && tp.Attempt == attempt
                          && tp.TagId == tagId
                          && tp.CourseKnown == courseKnown
                          && t.Type == templateType
                          && l.Name == languageCode
                          && t.ModuleId == crmModuleId
                    select t
                )
                .Distinct()
                .ProjectTo<TemplateDetailsDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(ct);

            if (argument != null)
            {
                templateArgument = argument;
            }
            else
            {
                day = 1;
                attempt = 1;
                templateArgument = await _context.TemplateProposals
                    .Include(tp => tp.TemplateProposalTemplates.Where(tpt => !tpt.IsDeleted))
                    .ThenInclude(tpt => tpt.Template)
                    .ThenInclude(t => t.Language)
                    .Where(tp => tp.ProcessType == (processType == ProcessType.Sale ? ProcessType.Records2 : processType)
                                 && tp.Day == day 
                                 && tp.Attempt == attempt 
                                 && tp.TagId == tagId
                                 && tp.CourseKnown == courseKnown)
                    .SelectMany(tp => tp.TemplateProposalTemplates.Select(tpt => tpt.Template))
                    .Where(t => t.Type == templateType 
                                && t.Language.Name == languageCode 
                                && t.ModuleId == crmModuleId 
                                && !t.IsDeleted)
                    .ProjectTo<TemplateDetailsDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(ct);
            }
        }

        return templateArgument;
    }

    private Colour GetValidColour(Colour colour)
    {
        if (colour == Colour.Grey || colour == Colour.GreenYellow)
            return Colour.Yellow;

        return colour;
    }

    private async Task<int> GetValidDay(Process process, ProcessSetting processSettings)
    {
        var day = await _processesService.GetProcessDay(process);

        if (day > processSettings.MaxDays)
            return processSettings.MaxDays;
        
        return day;
    }

    private int GetValidAttempt(Process process, ProcessSetting processSettings)
    {
        var attempts = process.Actions
            .Count(a => a.Type == ActionType.Call
                        && a.Date.Date == _dateTime.Now.Date &&
                        a.Outcome != ActionOutcome.WrongCall);

        if (attempts >= processSettings.MaxAttempts)
            return processSettings.MaxAttempts;
        
        return attempts == 0 ? 1 : attempts;
    }

    private async Task<string> GetValidLanguageCode(Process process, User user, CancellationToken cancellationToken)
    {
        var languageCode = process.Contact.ContactLanguages
            .FirstOrDefault(l => l.IsDefault)?.Language?.LocaleCode ?? "es";

        if (languageCode == null)
            languageCode = (await _context.Languages
                .FirstOrDefaultAsync(l => l.Name == user.Employee.Nationality, cancellationToken))?.LocaleCode 
                           ?? _configuration["Constants:DefaultLanguageCode"];

        return languageCode;
    }

    private async Task<ProcessSetting> GetProcessSettings(Process process, CancellationToken cancellationToken)
    {
        var containsType = await _context.ProcessSettings
            .Select(ps => ps.ProcessType)
            .ContainsAsync(process.Type, cancellationToken);

        var processSettings = new ProcessSetting(); 
        
        var foundProcessSettings = await _context.ProcessSettings
            .FirstOrDefaultAsync(ps => containsType && ps.ProcessType == process.Type 
                                       || !containsType && ps.ProcessType == null, cancellationToken);
        
        if (foundProcessSettings is not null)
        {
            processSettings = foundProcessSettings;
        }
        
        return processSettings;
    }
}