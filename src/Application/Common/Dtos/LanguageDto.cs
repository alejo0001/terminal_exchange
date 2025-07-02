using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class LanguageDto : IMapFrom<Language>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string NativeName { get; set; }
    public string DateFormat { get; set; }
    public string Currency { get; set; }
    public string FlagCode { get; set; }
}