using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Proposals.Queries.GetTemplateProposal;

[Authorize(Roles = "Administrador")]
public class GetTemplateProposalQuery: IRequest<TemplateProposalDto>
{
    public int TempateProposalId { get; set; }
}

public class GetTemplateProposalQueryHandler : IRequestHandler<GetTemplateProposalQuery, TemplateProposalDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTemplateProposalQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TemplateProposalDto> Handle(GetTemplateProposalQuery request, CancellationToken cancellationToken)
    {

        var templateProposal = await _context.TemplateProposals
            .FirstOrDefaultAsync(tp => tp.Id == request.TempateProposalId && !tp.IsDeleted, cancellationToken);

        if (templateProposal is null)
        {
            throw new NotFoundException("TemplateProposal not found!");
        }

        return   _mapper.Map<TemplateProposalDto>(templateProposal);
    }
}