using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CrmAPI.Application.Templates.Queries.GetTemplateId;

public class GetTemplateIdQuery : IRequest<int>
{
    public string? ProcessType { get; set; }
    public string? TemplateType { get; set; }
    public string? LanguageCode { get; set; }
    public int Day { get; set; }
    public int Attempt { get; set; }
    public string? Colour { get; set; }
    public Boolean CourseKnown { get; set; }
}

public class GetTemplateIdQueryHandler : IRequestHandler<GetTemplateIdQuery, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly  IOrganizationNodeExplorerService _organizationNodeExplorerService;
    private readonly ICurrentUserService _currentUserService;
    public GetTemplateIdQueryHandler(IApplicationDbContext context, IConfiguration configuration, IOrganizationNodeExplorerService organizationNodeExplorerService, ICurrentUserService currentUserService)
    {
        _context = context;
        _configuration = configuration;
        _organizationNodeExplorerService = organizationNodeExplorerService;
        _currentUserService = currentUserService;
    }

    public async Task<int> Handle(GetTemplateIdQuery request, CancellationToken cancellationToken)
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


        ProcessType processType = Enum.Parse<ProcessType>(request.ProcessType!, true);
        TemplateType templateType = Enum.Parse<TemplateType>(request.TemplateType!, true);
        Colour colour = Enum.Parse<Colour>(request.Colour!, true);
        int crmModuleId = Int32.Parse(_configuration["Constants:CRMModuleId"]!);
            
        return await _context.TemplateProposals
            .Where(tp => !tp.IsDeleted)
            .Include(tp => tp.TemplateProposalTemplates.Where(tpt => !tpt.IsDeleted))
            .ThenInclude(tpt => tpt.Template)
            .ThenInclude(t => t.Language)
            .Where(tp => tp.ProcessType == (processType == ProcessType.Sale ? ProcessType.Records : processType)
                         && tp.Day == request.Day && tp.Attempt == request.Attempt && tp.TagId == tagId
                         && tp.Colour == colour && tp.CourseKnown == request.CourseKnown)
            .SelectMany(tp => tp.TemplateProposalTemplates
                .Where(tpt => !tpt.IsDeleted)
                .Select(tpt => tpt.Template))
            .Where(t => t.Type == templateType && t.Language.Name == request.LanguageCode && t.ModuleId == crmModuleId && !t.IsDeleted)
            .Select(t => t.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}