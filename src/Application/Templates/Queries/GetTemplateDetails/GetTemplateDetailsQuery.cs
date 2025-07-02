using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Templates.Queries.GetTemplateDetails;

public class GetTemplateDetailsQuery : IRequest<TemplateDetailsDto>
{
    public int TemplateId { get; set; }
}
    
public class GetTemplateDetailsQueryHandler : IRequestHandler<GetTemplateDetailsQuery, TemplateDetailsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTemplateDetailsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
        
    public async Task<TemplateDetailsDto> Handle(GetTemplateDetailsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Templates
            .Include(t => t.Language)
            .Include(t => t.TemplateProposalTemplates.Where(tpt => !tpt.IsDeleted))
            .ThenInclude(t => t.TemplateProposal)
            .Where(t => t.Id == request.TemplateId && !t.IsDeleted)
            .ProjectTo<TemplateDetailsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}