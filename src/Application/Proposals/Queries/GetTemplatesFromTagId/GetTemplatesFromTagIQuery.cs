using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Mappings;
using CrmAPI.Application.Common.Models;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CrmAPI.Application.Common.Security;

namespace CrmAPI.Application.Proposals.Queries.GetTemplatesFromTagId;

// LISTADO DE TODAS LAS PLANTILLAS DE UN EQUIPO Y DE UN TIPO DETERMINADO

[Authorize(Roles = "Administrador")]
public class GetTemplatesFromTagIdAndProcessTypeQuery:  IRequest<List<TemplateDto>>
{ 
    public int TagId { get; set; }
}

public class GetTemplatesFromTagIdQueryHandler : IRequestHandler<GetTemplatesFromTagIdAndProcessTypeQuery, List<TemplateDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTemplatesFromTagIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<TemplateDto>> Handle(GetTemplatesFromTagIdAndProcessTypeQuery request, CancellationToken cancellationToken)
    {

        var set =  _context.Templates
           .Where(tpt => tpt.TagId == request.TagId)
           .Where(tpt => !tpt.IsDeleted)
           .AsQueryable();

        return await Task.Run(() => set.ProjectTo<TemplateDto>(_mapper.ConfigurationProvider)
           .ToList(), cancellationToken);
    }
}