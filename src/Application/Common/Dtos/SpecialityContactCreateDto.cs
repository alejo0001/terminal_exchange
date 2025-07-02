using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class SpecialityContactCreateDto : IMapFrom<ContactSpeciality>
{
    public int SpecialityId { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<SpecialityContactCreateDto, ContactSpeciality>()
            .ForMember(d => d.ContactId, opt =>
                opt.MapFrom(dom => 0));
        
        profile.CreateMap<SpecialityContactCreateDto, CroupierAPI.Contracts.Dtos.SpecialityContactCreateDto>().ReverseMap();
    }
}