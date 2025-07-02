using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Flows.Commands.DuplicateFlow;

[Authorize(Roles = "Usuario")]
public class DuplicateFlowCommand: FlowDuplicateDto, IRequest<Unit>
{ }

[UsedImplicitly]
public class DuplicateFlowCommandHandler : IRequestHandler<DuplicateFlowCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public DuplicateFlowCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(DuplicateFlowCommand request, CancellationToken ct)
    {
        var templateProposals = await _context.TemplateProposals
            .Where(tp => !tp.IsDeleted)
            .Where(tp => tp.ProcessType == request.OriginProcessType)
            .Where(tp => tp.TagId == request.OriginTagId)
            .Include(tp => tp.TemplateProposalTemplates
                .Where(tpt => !tpt.IsDeleted))
            .ThenInclude(tpt => tpt.Template)
            .ToListAsync(ct);

        var templateDictionary = new Dictionary<int, int>();

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {

            foreach (var templateProposal in templateProposals)
            {
                var newTemplateProposal = _mapper.Map<TemplateProposal>(templateProposal);

                var storedTemplateProposal = await SaveTemplateProposal(newTemplateProposal, request);
                await _context.SaveChangesAsync(ct);

                foreach (var templateProposalTemplate in templateProposal.TemplateProposalTemplates)
                {
                    var newTemplate = _mapper.Map<Template>(templateProposalTemplate.Template);

                    if (!templateDictionary.ContainsKey(templateProposalTemplate.Template.Id))
                    {
                        var storedTemplate = await SaveTemplate(newTemplate, request);
                        await _context.SaveChangesAsync(ct);

                        templateDictionary.Add(templateProposalTemplate.Template.Id, storedTemplate.Id);
                    }

                    var templateId = templateDictionary[templateProposalTemplate.Template.Id];
                    var templateProposalId = storedTemplateProposal.Id;

                    await SaveTemplateProposalTemplate(templateId, templateProposalId);
                    await _context.SaveChangesAsync(ct);
                }
            }

            await transaction.CommitAsync(ct);
        }
        catch (Exception)
        {
            // ReSharper disable once MethodSupportsCancellation
#pragma warning disable CA2016
            await transaction.RollbackAsync();
#pragma warning restore CA2016

            throw;
        }

        return Unit.Value;
    }

    private Task<TemplateProposal> SaveTemplateProposal(TemplateProposal originTemplateProposal, FlowDuplicateDto request)
    {

        var templateProposal = new TemplateProposal
        {
            Name = BuildName(originTemplateProposal.Name, request.TagName),
            ProcessType = request.ProcessType,
            Day = originTemplateProposal.Day,
            Attempt = originTemplateProposal.Attempt,
            Colour = originTemplateProposal.Colour,
            CourseKnown = originTemplateProposal.CourseKnown,
            CourseTypeId = originTemplateProposal.CourseTypeId,
            HasToSendEmail = originTemplateProposal.HasToSendEmail,
            HasToSendWhatsApp = originTemplateProposal.HasToSendWhatsApp,
            TagId = request.TagId,
        };

        _context.TemplateProposals.Add(templateProposal);

        return Task.FromResult(templateProposal);
    }
        
    private Task<TemplateProposalTemplate> SaveTemplateProposalTemplate(int templateId, int templateProposalId)
    {

        var templateProposalTemplate = new TemplateProposalTemplate
        {
            TemplateId = templateId,
            TemplateProposalId = templateProposalId
        };

        _context.TemplateProposalTemplates.Add(templateProposalTemplate);

        return Task.FromResult(templateProposalTemplate);
    }

    private Task<Template> SaveTemplate(Template originTemplate, FlowDuplicateDto request)
    {

        var template = new Template
        {
            Label = originTemplate.Label,
            Name = BuildName(originTemplate.Name, request.TagName),
            Subject = originTemplate.Subject,
            Body = originTemplate.Body,
            Type = originTemplate.Type,
            LanguageId = originTemplate.LanguageId,
            CourseNeeded = originTemplate.CourseNeeded,
            ModuleId = originTemplate.ModuleId,
            TagId = request.TagId,
            Order = originTemplate.Order
        };
  
        _context.Templates.Add(template);

        return Task.FromResult(template);
    }

    private string BuildName(string originName, string tagName)
    {

        var name = originName;

        var splitName = originName.Split(']');

        if (splitName.Length > 1)
        {
            name = "[" + tagName + "]" + splitName.Last();
        }
        else
        {
            name = "[" + tagName + "] " + name;
        }

        return name;
    }

}