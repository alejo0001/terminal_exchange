using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ContactLanguageCreateDto : IMapFrom<ContactLanguage>
{
    public int ContactId { get; set; }
    public int LanguageId { get; set; }
    public bool IsDefault { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<ContactLanguageCreateDto, ContactLanguage>();
        profile.CreateMap<ContactLanguageCreateDto, CroupierAPI.Contracts.Dtos.ContactLanguageCreateDto>().ReverseMap();
    }
}