using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Proposals.Queries.GetTemplatesFromProposal.GetTemplatesFromTemplateProposal;


// OBTENER TODAS LAS PLANTILLAS DE UNO DE LOS PROPOSALS POR PROPOSALID (DETALLES DE UN PROPOSAL)

[Authorize(Roles = "Administrador")]
public class GetTemplatesFromTemplateProposalQuery:  IRequest<List<TemplateProposalTemplateDto>>
{
    public int TempateProposalId { get; set; }
}


public class GetTemplatesFromProposalQueryHandler : IRequestHandler<GetTemplatesFromTemplateProposalQuery, List<TemplateProposalTemplateDto>>
{
    
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTemplatesFromProposalQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<TemplateProposalTemplateDto>> Handle(GetTemplatesFromTemplateProposalQuery request, CancellationToken cancellationToken)
    {
        var set =  _context.TemplateProposalTemplates
            .Include(tpt => tpt.Template)
            .Where(tpt => tpt.TemplateProposalId == request.TempateProposalId && !tpt.IsDeleted)
            .Where(tpt => !tpt.Template.IsDeleted)
            .AsQueryable();
        
        return await Task.Run(() => set.ProjectTo<TemplateProposalTemplateDto>(_mapper.ConfigurationProvider)
            .ToList(), cancellationToken);
    }
}