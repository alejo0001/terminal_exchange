using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ContactLanguageDto : IMapFrom<ContactLanguage>
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public int LanguageId { get; set; }
    public LanguageDto Language { get; set; }
    public bool IsDefault { get; set; }
}