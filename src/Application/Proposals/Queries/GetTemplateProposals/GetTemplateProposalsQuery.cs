using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Mappings;
using CrmAPI.Application.Common.Models;
using CrmAPI.Application.Common.Security;
using MediatR;
using Techtitute.DynamicFilter.Services;

namespace CrmAPI.Application.Proposals.Queries.GetTemplateProposals;

[Authorize(Roles = "Administrador")]
public class GetTemplateProposalsQuery: IRequest<PaginatedList<TemplateProposalDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10000;
    public string QueryParams { get; set; } = "";
}


public class GetTemplateProposalsQueryHandler : IRequestHandler<GetTemplateProposalsQuery, PaginatedList<TemplateProposalDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTemplateProposalsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<TemplateProposalDto>> Handle(GetTemplateProposalsQuery request, CancellationToken cancellationToken)
    {
        //TODO: hay que filtrar por los proposals que puede ver el usuario logeado
        var set =  _context.TemplateProposals
            .Where(t => !t.IsDeleted)
            .ApplyParameters(_context, request.QueryParams);

        return await set.ProjectTo<TemplateProposalDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}