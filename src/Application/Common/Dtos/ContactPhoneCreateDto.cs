using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ContactPhoneCreateDto : IMapFrom<ContactPhone>
{
    public int ContactId { get; set; }
    public int PhoneTypeId { get; set; }
    public string Phone { get; set; }
    public string PhonePrefix { get; set; }
    public bool IsDefault { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<ContactPhoneCreateDto, ContactPhone>();
        profile.CreateMap<ContactPhoneCreateDto, CroupierAPI.Contracts.Dtos.ContactPhoneCreateDto>().ReverseMap();
    }
}