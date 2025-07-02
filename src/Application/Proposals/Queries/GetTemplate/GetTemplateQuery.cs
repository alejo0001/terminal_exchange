using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Proposals.Queries.GetTemplateDetails;

[Authorize(Roles = "Administrador")]
public class GetTemplateDetailsQuery:  IRequest<TemplateDetailsDto>
{ 
    public int TemplateId { get; set; }
}

public class GetTemplateDetailsQueryHandler : IRequestHandler<GetTemplateDetailsQuery,TemplateDetailsDto>
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
        var template = await _context.Templates
            .Include(t => t.Language)
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);
        
        if (template is null)
        {
            throw new NotFoundException("Template not found!");
        }
        return   _mapper.Map<TemplateDetailsDto>(template);
    }
}
