using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CrmAPI.Application.Templates.Queries.GetTemplateBundleProposal;

public class GetTemplateBundleProposalQuery: IRequest<TemplateBundleProposalViewModel>
{
    public int ProcessId { get; set; }
    public string LanguageCode { get; set; }
    public int? CourseId { get; set; }
    public bool? Response { get; set; }
}

public class GetTemplateBundleProposalQueryHandler : IRequestHandler<GetTemplateBundleProposalQuery, TemplateBundleProposalViewModel>
{
    private readonly IApplicationDbContext _context;
    private readonly IProcessesService _processesService;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly  IOrganizationNodeExplorerService _organizationNodeExplorerService;

    public GetTemplateBundleProposalQueryHandler(IApplicationDbContext context, IProcessesService processesService,
        IConfiguration configuration, IMapper mapper, IOrganizationNodeExplorerService organizationNodeExplorerService)
    {
        _context = context;
        _processesService = processesService;
        _configuration = configuration;
        _mapper = mapper;
        _organizationNodeExplorerService = organizationNodeExplorerService;
    }

    public async Task<TemplateBundleProposalViewModel> Handle(GetTemplateBundleProposalQuery request, CancellationToken cancellationToken)
    {
        // Buscamos el proceso que hay que gestionar
        var process = await _context.Processes
            .Include(p => p.User.Employee)
            .Include(p => p.Actions)
            .FirstOrDefaultAsync(p => p.Id == request.ProcessId, cancellationToken);
        
        
        var user = await _context.Users
            .Include(u => u.Employee)
            .ThenInclude(e => e.CurrentOrganizationNode)
            .ThenInclude(o => o.OrganizationNodeTags)
            .ThenInclude(ot => ot.Tag)
            .FirstOrDefaultAsync(u => u.Id == process.UserId, 
                cancellationToken);

        // Encotramos el TagId del nodo más próximo en el que se encuentra el usuario, para localizar el flujo del
        // equipo al que pertenece
        var tagId = await _organizationNodeExplorerService
            .GetTagIdNode(user.Employee.CurrentOrganizationNode, _configuration["Constants:TagCrmFlows"]!);
            
        var currentDay = await _processesService.GetProposalProcessDay(process);

        var attempt = await _processesService.ComputeAttempts(process.Actions, process.Type);

        if (attempt < 1)
            attempt = 1;

        //TODO: hay que pensar esto mejor y refactorizarlo
        var colour = process.Colour ?? Colour.Yellow;

        if (colour == Colour.Grey)
        {
            colour = Colour.Yellow;
        }
        
        if (request.Response == true)
        {
            colour = Colour.Green;
        }
        else
        {
            if (process.Colour == Colour.Green)
            {
                colour = Colour.GreenYellow;
            }
        }
        

        // TODO: Hay que mejorar esto. Esto lo hacemos para devolver en los cupones
        // las plantillas verdes del día 1, intento 1 si el proceso siempre ha sido amarillo
      /*  if (process.Type != ProcessType.Records 
            && process.Colour == Colour.Yellow 
            && request.Response == true)
        {
            currentDay = 1;
            attempt = 1;
        }*/
       

        IQueryable<TemplateProposal> proposalQuery = _context.TemplateProposals
            .Where(tp => !tp.IsDeleted)
            .Include(tp => tp.TemplateProposalTemplates
                .Where(tpt => !tpt.IsDeleted))
            .ThenInclude(tpt => tpt.Template)
            .ThenInclude(t => t.Language)
            .Where(tp =>
                tp.TagId == tagId 
                && tp.Colour == colour 
                && tp.ProcessType == (process.Type == ProcessType.Sale ? ProcessType.Records : process.Type)
                && tp.IsDeleted == false
            );

        proposalQuery = proposalQuery.Where(tp =>
                tp.Day == currentDay &&
                tp.Attempt == attempt);
            
        if (request.CourseId != null)
        {
            proposalQuery = proposalQuery.Where(tp => tp.CourseKnown);
        }
        else
        {
            proposalQuery = proposalQuery.Where(tp => !tp.CourseKnown);
        }

        IQueryable<Template> templateQueryable = proposalQuery
            .SelectMany(tp => tp.TemplateProposalTemplates.Where(tpt => !tpt.IsDeleted)
                .Select(tpt => tpt.Template))
            .Where(t => t.Language.Name == request.LanguageCode && !t.IsDeleted);

        var emailTemplate = await templateQueryable
            .Where(t => t.Type == TemplateType.Email)
            .ProjectTo<TemplateDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
        var whatsAppTemplate = await templateQueryable
            .Where(t => t.Type == TemplateType.WhatsApp)
            .ProjectTo<TemplateDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        var proposal = (await proposalQuery.FirstOrDefaultAsync(cancellationToken));

        return new TemplateBundleProposalViewModel
        {
            EmailTemplate = emailTemplate,
            WhatsAppTemplate = whatsAppTemplate,
            HasToSendEmail = proposal?.HasToSendEmail ?? false,
            HasToSendWhatsapp = proposal?.HasToSendWhatsApp ?? false
        };
    }
}