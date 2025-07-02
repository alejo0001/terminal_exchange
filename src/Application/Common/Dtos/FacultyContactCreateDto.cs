using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class FacultyContactCreateDto : IMapFrom<ContactFaculty>
{
    public int? FacultyId { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<FacultyContactCreateDto, ContactFaculty>()
            .ForMember(d => d.ContactId, opt =>
                opt.MapFrom(dom => 0));
        profile.CreateMap<FacultyContactCreateDto, CroupierAPI.Contracts.Dtos.FacultyContactCreateDto>().ReverseMap();
    }
}