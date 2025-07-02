using System;
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
using Microsoft.Extensions.Configuration;

namespace CrmAPI.Application.Templates.Queries.GetTemplate;

public class GetTemplateQuery : IRequest<TemplateDto>
{
    public string? ProcessType { get; set; }
    public string? TemplateType { get; set; }
    public string? LanguageCode { get; set; }
    public int Day { get; set; }
    public int Attempt { get; set; }
    public string? Colour { get; set; }
    public Boolean CourseKnown { get; set; }
}

public class GetTemplateQueryHandler : IRequestHandler<GetTemplateQuery, TemplateDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly  IOrganizationNodeExplorerService _organizationNodeExplorerService;
    private readonly ICurrentUserService _currentUserService;
    public GetTemplateQueryHandler(IApplicationDbContext context, IMapper mapper, IConfiguration configuration, IOrganizationNodeExplorerService organizationNodeExplorerService, ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _configuration = configuration;
        _organizationNodeExplorerService = organizationNodeExplorerService;
        _currentUserService = currentUserService;
    }

    public async Task<TemplateDto?> Handle(GetTemplateQuery request, CancellationToken cancellationToken)
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
        
        var processType = Enum.Parse<ProcessType>(request.ProcessType!, true);
        var templateType = Enum.Parse<TemplateType>(request.TemplateType!, true);
        var colour = Enum.Parse<Colour>(request.Colour!, true);
        var crmModuleId = Int32.Parse(_configuration["Constants:CRMModuleId"]!);
            
        return await _context.TemplateProposals
            .Include(tp => tp.TemplateProposalTemplates.Where(tpt => !tpt.IsDeleted))
                .ThenInclude(tpt => tpt.Template)
                    .ThenInclude(t => t.Language)
            .Where(tp => tp.ProcessType == (processType == ProcessType.Sale ? ProcessType.Records : processType)
                         && tp.Day == request.Day && tp.Attempt == request.Attempt && tp.TagId == tagId
                         && tp.Colour == colour && tp.CourseKnown == request.CourseKnown)
            .SelectMany(tp => tp.TemplateProposalTemplates.Select(tpt => tpt.Template))
            .Where(t => t.Type == templateType && t.Language.Name == request.LanguageCode && t.ModuleId == crmModuleId && !t.IsDeleted)
            .ProjectTo<TemplateDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}