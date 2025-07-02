using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class PhoneTypeDto : IMapFrom<PhoneType>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Label { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<PhoneType, PhoneTypeDto>();
        profile.CreateMap<PhoneTypeDto, PhoneType>();
    }
}