using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ContactTitleCreateDto : IMapFrom<ContactTitle>
{
    public int ContactId { get; set; }
    public int TitleTypeId { get; set; }
    public string AcademicInstitution { get; set; }
    public string Degree { get; set; }
    public bool IsDeleted { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ContactTitleCreateDto, ContactTitle>();
        
        profile.CreateMap<ContactTitleCreateDto, CroupierAPI.Contracts.Dtos.ContactTitleCreateDto>().ReverseMap();
    }
}