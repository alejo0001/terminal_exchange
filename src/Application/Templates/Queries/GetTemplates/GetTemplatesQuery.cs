using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CrmAPI.Application.Templates.Queries.GetTemplates;

public class GetTemplatesQuery : IRequest<List<TemplateDto>>
{
    public string? ProcessType { get; set; }
    public string? LanguageCode { get; set; }
    public string? TemplateType { get; set; }
}

public class GetTemplatesQueryHandler : IRequestHandler<GetTemplatesQuery, List<TemplateDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly  IOrganizationNodeExplorerService _organizationNodeExplorerService;
    private readonly ICurrentUserService _currentUserService;
    public GetTemplatesQueryHandler(IApplicationDbContext context, IMapper mapper, IConfiguration configuration,
        IOrganizationNodeExplorerService organizationNodeExplorerService, ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _configuration = configuration;
        _organizationNodeExplorerService = organizationNodeExplorerService;
        _currentUserService = currentUserService;
    }

    public async Task<List<TemplateDto>> Handle(GetTemplatesQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .ThenInclude(e => e.CurrentOrganizationNode)
            .ThenInclude(o => o.OrganizationNodeTags)
            .ThenInclude(ot => ot.Tag)
            .FirstOrDefaultAsync(u => u.Employee.CorporateEmail == _currentUserService.Email, 
                cancellationToken);

        // Encotramos el TagId del nodo más próximo en el que se encuentra el usuario, para enviarle las plantillas del
        // equipo al que pertenece
        var tagId = await _organizationNodeExplorerService
            .GetTagIdNode(user!.Employee.CurrentOrganizationNode, _configuration["Constants:TagCrmFlows"]!);

        if (tagId == 0)
        {
            throw new NotFoundException("User has not TagId");
        }

        ProcessType processType = Enum.Parse<ProcessType>(request.ProcessType!, true);
        TemplateType templateType = Enum.Parse<TemplateType>(request.TemplateType!, true);
        int crmModuleId = Int32.Parse(_configuration["Constants:CRMModuleId"]!);
         
        //TODO: refactorizar esta consulta zurda
        return await _context.TemplateProposals
            .Include(tp => tp.TemplateProposalTemplates)
            .ThenInclude(tpt => tpt.Template)
            .ThenInclude(t => t.Language)
            .Where(tp => tp.TagId == tagId && !tp.IsDeleted
                         && tp.ProcessType == (processType == ProcessType.Sale ? ProcessType.Records : processType))
            .SelectMany(tp => tp.TemplateProposalTemplates.Where(tpt => !tpt.IsDeleted)
                .Select(tpt => tpt.Template).Where(t => !t.IsDeleted))
            .Where(t => t.Type == templateType 
                        && t.Language.Name == request.LanguageCode && t.ModuleId == crmModuleId)
            .Distinct()
            // se está ordenando de forma que mostramos las plantillas del día 0 el primero y las del día 99 las últimas,
            // después ordenamos por colores, amerillo, verde y verde amarillo, y por último ordenamos por días e intentos.
            // (las plantillas del día 99 serrían plantillas como 'teléfono erroneo')
            .OrderByDescending(t => t.TemplateProposalTemplates
                .Select(tpt => tpt.TemplateProposal)
                .FirstOrDefault().Day == 0)
            .ThenBy(t => t.TemplateProposalTemplates
                .Select(tpt => tpt.TemplateProposal)
                .FirstOrDefault().Day == 99)
            .ThenByDescending(t => t.TemplateProposalTemplates
                .Select(tpt => tpt.TemplateProposal)
                .FirstOrDefault().Colour == Colour.Yellow)
            .ThenBy(t => t.TemplateProposalTemplates
                .Select(tpt => tpt.TemplateProposal)
                .FirstOrDefault().Colour)
            .ThenBy(t => t.TemplateProposalTemplates
                .Select(tpt => tpt.TemplateProposal)
                .FirstOrDefault().Day)
            .ThenBy(t => t.TemplateProposalTemplates
                .Select(tpt => tpt.TemplateProposal)
                .FirstOrDefault().Attempt)
            .ProjectToListAsync<TemplateDto>(_mapper.ConfigurationProvider);
    }
}