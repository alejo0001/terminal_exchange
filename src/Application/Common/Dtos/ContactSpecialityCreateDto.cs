using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ContactSpecialityCreateDto : IMapFrom<ContactSpeciality>
{
    public int SpecialityId { get; set; }
    public int ContactId { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<ContactFacultyCreateDto, ContactFaculty>();
    }
}