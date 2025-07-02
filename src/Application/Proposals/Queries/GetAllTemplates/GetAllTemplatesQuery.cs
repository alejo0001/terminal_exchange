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

namespace CrmAPI.Application.Proposals.Queries.GetAllTemplates;

[Authorize(Roles = "Administrador")]
public class GetAllTemplatesQuery: IRequest<PaginatedList<TemplateDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10000;
    public string QueryParams { get; set; } = "";
}


public class GetAllTemplatesQueryHandler : IRequestHandler<GetAllTemplatesQuery, PaginatedList<TemplateDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllTemplatesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<TemplateDto>> Handle(GetAllTemplatesQuery request, CancellationToken cancellationToken)
    {
        var set =  _context.Templates
            .Where(t => !t.IsDeleted)
            .ApplyParameters(_context, request.QueryParams);

        return await set.ProjectTo<TemplateDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}