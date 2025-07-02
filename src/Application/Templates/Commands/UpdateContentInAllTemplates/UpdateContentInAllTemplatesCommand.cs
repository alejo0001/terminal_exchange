using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using CrmAPI.Application.Templates.Commands.UpdateContentInAllTemplates;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Proposals.Commands.UpdateContentInAllTemplates;

[Authorize(Roles = "Administrador")]
public class UpdateContentInAllTemplatesCommand: IRequest<AffectedTemplatesViewModel>
{
    public string ActualContent { get; set; }
    public string NewContent { get; set; } = "";
}

public class UpdateContentInAllTemplatesCommandHandler : IRequestHandler<UpdateContentInAllTemplatesCommand, AffectedTemplatesViewModel>
{
    
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateContentInAllTemplatesCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<AffectedTemplatesViewModel> Handle(UpdateContentInAllTemplatesCommand request, CancellationToken cancellationToken)
    {
        var templates = _context.Templates
            .Include(t => t.TemplateProposalTemplates)
                .ThenInclude(tpt => tpt.TemplateProposal)
            .Where(t => t.Body.Contains(request.ActualContent))
            .ToList();
            
            templates.ForEach(p => p.Body = p.Body.Replace(request.ActualContent, request.NewContent));

        await _context.SaveChangesAsync(cancellationToken);

        return new AffectedTemplatesViewModel()
        {
            OldContentValue = request.ActualContent,
            NewContentValue = request.NewContent,
            AffectedTemplates = templates.Select(t => t.Id).ToList(),
            AffectedTemplateLanguages = templates.Select(t => t.LanguageId).Distinct().ToList()
        };
    }
}