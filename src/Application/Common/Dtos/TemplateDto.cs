using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class TemplateDto : IMapFrom<Template>
{
    public int Id { get; set; }
    public string? Label { get; set; }
    public string? Name { get; set; }
    public string? LanguageCode { get; set; }
    public bool CourseNeeded { get; set; }
    public string? Type { get; set; }
    public int? LanguageId { get; set; }
    public LanguageDto? Language { get; set; }
    public int? TagId { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public int? day { get; set; }
    public int? attempt { get; set; }
    public bool IsDeleted { get; set; }
    public int? Order { get; set; }
    public string? fromEmail { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Template, TemplateDto>()
            .ForMember(d => d.Type, opt =>
                opt.MapFrom(dom => dom.Type.ToString().ToLowerInvariant()))
            .ForMember(dto => dto.LanguageCode, op => op.MapFrom(dom => dom.Language.Name));
    }
}