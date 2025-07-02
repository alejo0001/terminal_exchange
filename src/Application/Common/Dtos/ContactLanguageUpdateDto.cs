using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ContactLanguageUpdateDto : IMapFrom<ContactLanguage>
{
    public int? Id { get; set; }
    public int ContactId { get; set; }
    public int LanguageId { get; set; }
    public bool IsDefault { get; set; }
    public bool IsDeleted { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<ContactLanguageUpdateDto, ContactLanguage>();
    }
}