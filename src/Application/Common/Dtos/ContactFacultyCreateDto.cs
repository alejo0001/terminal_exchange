using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ContactFacultyCreateDto : IMapFrom<ContactFaculty>
{
    public int FacultyId { get; set; }
    public int ContactId { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<ContactFacultyCreateDto, ContactFaculty>();
    }
}