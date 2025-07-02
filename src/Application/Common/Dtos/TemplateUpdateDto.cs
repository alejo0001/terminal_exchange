using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class TemplateUpdateDto : IMapFrom<Template>
{
    public int Id { get; set; }
    public string Label { get; set; }
    public string Name { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public TemplateType Type { get; set; }
    public string LanguageCode { get; set; }
    public bool CourseNeeded { get; set; }
    public int? TagId { get; set; }
    public int? Order { get; set; }
}