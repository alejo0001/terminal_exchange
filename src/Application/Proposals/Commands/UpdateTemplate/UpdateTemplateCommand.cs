using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Proposals.Commands.UpdateTemplate;

[Authorize(Roles = "Administrador")]
public class UpdateTemplateCommand: TemplateUpdateDto,  IRequest
{
}

public class UpdateTemplateCommandHandler : IRequestHandler<UpdateTemplateCommand>
{
    
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateTemplateCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(UpdateTemplateCommand request, CancellationToken ct)
    {
        
        var template = await _context.Templates.FirstOrDefaultAsync(t => t.Id == request.Id, ct);
        if (template == null)
        {
            throw new NotFoundException(nameof(template), request.Id);
        }

        var languageId = 1;
        var laguage = await _context.Languages.FirstOrDefaultAsync(l => l.Name == request.LanguageCode, ct);

        if (laguage is not null)
        {
            languageId = laguage.Id;
        }

        template.Label = request.Label; 
        template.Name = request.Name;
        template.Subject = request.Subject;
        template.Body = request.Body;
        template.Type = request.Type;
        template.LanguageId = languageId;
        template.CourseNeeded = request.CourseNeeded;
        template.ModuleId = 2;
        template.Order = request.Order;
        template.TagId = request.TagId;

        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}