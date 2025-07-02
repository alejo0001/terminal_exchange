using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ContactAddressUpdateDto : IMapFrom<ContactAddress>
{
    public int? Id { get; set; }
    public int ContactId { get; set; }
    public int AddressTypeId { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string CountryCode { get; set; }
    public string Province { get; set; }
    public string PostalCode { get; set; }
    public string Department { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsDefault { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<ContactAddressUpdateDto, ContactAddress>();
    }
}