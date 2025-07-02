using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ContactEmailCreateDto : IMapFrom<ContactEmail>
{
    public int ContactId { get; set; }
    public int EmailTypeId { get; set; }
    public string Email { get; set; }
    public bool IsDefault { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<ContactEmailCreateDto, ContactEmail>();
        profile.CreateMap<ContactEmailCreateDto, CroupierAPI.Contracts.Dtos.ContactEmailCreateDto>().ReverseMap();
    }
}