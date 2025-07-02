using System.Linq;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class TemplateDetailsDto : IMapFrom<Template>
{
    public int Id { get; set; }
    public string Label { get; set; }
    public string Name { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public string Colour { get; set; }
    public int Day { get; set; }
    public int Attempt { get; set; }
    public string Type { get; set; }
    public bool CourseNeeded { get; set; }
    public bool CourseKnown { get; set; }
    public int LanguageId { get; set; }
    public int TagId { get; set; }
    public string? Team { get; set; }
    public LanguageDto Language { get; set; }
    public int? Order { get; set; }
    public string? fromEmail { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Template, TemplateDetailsDto>()
            .ForMember(d => d.Team, template =>
                template.MapFrom(dom => dom.TagId == null ? "" : dom.Tag.Name))
            .ForMember(d => d.Type, opt =>
                opt.MapFrom(dom => dom.Type.ToString().ToLowerInvariant()))
            .ForMember(dto => dto.Colour, expression =>
                expression.MapFrom(template =>
                    template.TemplateProposalTemplates
                        .FirstOrDefault(t => !t.IsDeleted && !t.TemplateProposal.IsDeleted).TemplateProposal.Colour))
            .ForMember(dto => dto.Day, expression =>
                expression.MapFrom(template =>
                    template.TemplateProposalTemplates.FirstOrDefault().TemplateProposal.Day))
            .ForMember(dto => dto.Attempt, expression =>
                expression.MapFrom(template =>
                    template.TemplateProposalTemplates.FirstOrDefault().TemplateProposal.Attempt))
            .ForMember(dto => dto.CourseKnown, expression =>
                expression.MapFrom(template =>
                    template.TemplateProposalTemplates.FirstOrDefault().TemplateProposal.CourseKnown));
    }
}