using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class AddressTypeDto : IMapFrom<AddressType>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Label { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<AddressType, AddressTypeDto>();
        profile.CreateMap<AddressTypeDto, AddressType>();
    }
}