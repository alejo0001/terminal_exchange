using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace CrmAPI.Application.Proposals.Commands.CreateTemplate.cs;

[Authorize(Roles = "Administrador")]
public class CreateTemplateCommand: TemplateCreateDto, IRequest<int>
{
}

public class CreateTemplateCommandHandler : IRequestHandler<CreateTemplateCommand, int>
{
    
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
 
    public CreateTemplateCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<int> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        var newTemplate = _mapper.Map<Template>(request);
        newTemplate.ModuleId = 2;
        newTemplate.LanguageId = 1;

        if (request.LanguageCode is not null)
        {
            var language = await _context.Languages.FirstOrDefaultAsync(l => l.Name == request.LanguageCode, cancellationToken);

            if (language is not null)
            {
                newTemplate.LanguageId = language.Id;
            }
        }

        _context.Templates.Add(newTemplate);
        await _context.SaveChangesAsync(cancellationToken);
        return newTemplate.Id;
    }
}