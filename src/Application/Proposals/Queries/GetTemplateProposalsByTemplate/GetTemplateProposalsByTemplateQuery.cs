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

namespace CrmAPI.Application.Proposals.Queries.GetTemplateProposalsFromTemplate;


// LISTADO DE PROPOSALS QUE USAN UNA PLANTILLA EN CONCRETO

[Authorize(Roles = "Administrador")]
public class GetTemplateProposalsByTemplateQuery: IRequest<List<TemplateProposalDto>>
{
    public int TemplateId { get; set; }
}

public class GetTemplateProposalsFromTemplateQueryHandler : IRequestHandler<GetTemplateProposalsByTemplateQuery, List<TemplateProposalDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTemplateProposalsFromTemplateQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<List<TemplateProposalDto>> Handle(GetTemplateProposalsByTemplateQuery request, CancellationToken cancellationToken)
    {
        var proposalList = await _context.TemplateProposalTemplates
            .Include(tpt => tpt.Template)
            .Where(tpt => tpt.TemplateId == request.TemplateId)
            .Where(tpt => !tpt.Template.IsDeleted)
            .Where(tpt => !tpt.IsDeleted)
            .Select(tpt => tpt.TemplateProposalId)
            .ToListAsync(cancellationToken);

        var set =  _context.TemplateProposals
            .Where(tp => proposalList.Contains(tp.Id))
            .Where(tp => !tp.IsDeleted)
            .AsQueryable();
        
        return await Task.Run(() => set.ProjectTo<TemplateProposalDto>(_mapper.ConfigurationProvider)
            .ToList(), cancellationToken);

    }
}