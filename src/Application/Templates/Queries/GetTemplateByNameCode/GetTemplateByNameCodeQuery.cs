using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using IntranetMigrator.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CrmAPI.Application.Templates.Queries.GetTemplateByNameCode;

[Authorize]
public record GetTemplateByNameCodeQuery(
    string NameCode,
    int LanguageId,
    TemplateType Type)
    : IRequest<TemplateDto>;

public class GetTemplateByNameCodeQueryHandler : IRequestHandler<GetTemplateByNameCodeQuery, TemplateDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTemplateByNameCodeQuery> _logger;

    public GetTemplateByNameCodeQueryHandler(
        IApplicationDbContext context, 
        IMapper mapper,
        ILogger<GetTemplateByNameCodeQuery> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }
    
    public async Task<TemplateDto?> Handle(GetTemplateByNameCodeQuery request, CancellationToken ct)
    {
        try
        {
            return await _context.Templates
                .Where(t => t.Name.Contains(request.NameCode)
                            && t.LanguageId == request.LanguageId
                            && t.Type == request.Type
                            && t.TagId == 12
                            && !t.IsDeleted)
                .AsNoTracking()
                .ProjectTo<TemplateDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(ct);
        }
        catch (Exception e)
        {
            _logger.LogError("Error in GetTemplateByNameCodeQuery");
            return null;
        }
    }
}
