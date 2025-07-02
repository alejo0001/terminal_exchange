using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ReassignmentDto: IMapFrom<Reassignment>
{
    public int Id { get; set; }
    public int FromUserId { get; set; }
    public int ToUserId { get; set; }
    public int RassignerId { get; set; }
        
    public UserDto FromUser { get; set; }
    public UserDto ToUser { get; set; }
    public UserDto Reassigner { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Reassignment, ReassignmentDto>();
    }
}