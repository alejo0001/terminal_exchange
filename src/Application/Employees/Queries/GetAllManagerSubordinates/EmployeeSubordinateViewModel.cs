using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Employees.Queries.GetAllManagerSubordinates;

public class EmployeeSubordinateViewModel: IMapFrom<Employee>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string CountryName { get; set; }
    public string OrganizationNodeName { get; set; }
    public int UserId { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Employee, EmployeeSubordinateViewModel>()
            .ForMember(dto => dto.Name, op =>
                op.MapFrom(dom => dom.User.Name))
            .ForMember(dto => dto.Surname, op =>
                op.MapFrom(dom => dom.User.Surname))
            .ForMember(dto => dto.CountryName, op =>
                op.MapFrom(dom => dom.CurrentCountry.Name))
            .ForMember(dto => dto.OrganizationNodeName, op =>
                op.MapFrom(dom => dom.CurrentOrganizationNode.Name))
            .ForMember(dto => dto.UserId, op =>
                op.MapFrom(dom => dom.User.Id));
    }
}