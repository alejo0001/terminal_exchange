using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Flows.Queries.GetFlows;

public class GetFlowsQuery: IRequest<List<FlowDto>>
{
}

public class GetFlowsQueryHandler : IRequestHandler<GetFlowsQuery, List<FlowDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFlowsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
    }
        
    public async Task<List<FlowDto>> Handle(GetFlowsQuery request,
        CancellationToken cancellationToken)
    {
        
        var templateProposals = await _context.TemplateProposals
            .Where(tp => tp.TagId != null)
            .ToListAsync(cancellationToken);
        
        Dictionary<string, FlowDto> flowsDictionary = new Dictionary<string, FlowDto>();
        
        foreach (TemplateProposal templateProposal in templateProposals)
        {
            
            var key = $"{templateProposal.ProcessType}-{templateProposal.TagId}";
            
            if (!flowsDictionary.ContainsKey(key))
            {

                flowsDictionary.Add(key, new FlowDto());

                flowsDictionary[key].ProcessType = templateProposal.ProcessType;
                flowsDictionary[key].TagId = templateProposal.TagId;
                flowsDictionary[key].TotalSteps = 0;
            }
            
            flowsDictionary[key].TotalSteps++;
        }

        return flowsDictionary.Values.ToList();
    }
}